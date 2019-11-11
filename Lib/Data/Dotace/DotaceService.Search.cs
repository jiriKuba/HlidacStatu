﻿using Devmasters.Core;
using HlidacStatu.Lib.ES;
using Nest;
using System;

namespace HlidacStatu.Lib.Data.Dotace
{
    public partial class DotaceService
    {
        static string regex = "[^/]*\r\n/(?<regex>[^/]*)/\r\n[^/]*\r\n";
        static System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
                    | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        static System.Text.RegularExpressions.Regex regFindRegex = new System.Text.RegularExpressions.Regex(regex, options);


        // podle čeho můžeme vyhledat
        static string[] queryShorcuts = new string[] {
                "ico:",
                "jmeno:",
                "ucel:",
                "projekt:",
                "osobaid:","holding:",
                "id:"
            };
        static string[] queryOperators = new string[] { "AND", "OR" };


        public QueryContainer GetSimpleQuery(string query)
        {
            return GetSimpleQuery(new DotaceSearchResult() { Q = query, Page = 1 });
        }
        public QueryContainer GetSimpleQuery(DotaceSearchResult searchdata)
        {
            var query = searchdata.Q;
            
            //fix field prefixes
            //ds: -> 
            Lib.Search.Rule[] rules = new Lib.Search.Rule[] {
                   new Lib.Search.Rule(@"osobaid:(?<q>((\w{1,} [-]{1} \w{1,})([-]{1} \d{1,3})?)) ","ico"),
                   new Lib.Search.Rule(@"holding:(?<q>(\d{1,8})) (\s|$){1,}","ico"),
                   new Lib.Search.Rule(@"ico:","prijemceIco:"),
                   new Lib.Search.Rule("jmeno:","prijemceJmenoPrijemce:"),
                   new Lib.Search.Rule("ucel:","(obdobiDotaceTitulNazev:${q} OR obdobiUcelZnakNazev:${q}) "),
                   new Lib.Search.Rule("projekt:","dotaceProjektNazev:"),
                   new Lib.Search.Rule("id:","idDotace:"),
            };

            string modifiedQ = query; // Search.Tools.FixInvalidQuery(query, queryShorcuts, queryOperators) ?? "";
                                      //check invalid query ( tag: missing value)

            if (searchdata.LimitedView)
                modifiedQ = Lib.Search.Tools.ModifyQueryAND(modifiedQ, "onRadar:true");

            var qc = Lib.Search.Tools.GetSimpleQuery<Dotace>(modifiedQ, rules); ;

            return qc;

        }

        public DotaceSearchResult SimpleSearch(string query, int page, int pagesize, int order,
            bool withHighlighting = false,
            bool limitedView = false,
            AggregationContainerDescriptor<Dotace> anyAggregation = null)
        {
            return SimpleSearch(new DotaceSearchResult()
            {
                Q = query,
                Page = page,
                PageSize = pagesize,
                LimitedView = limitedView,
                Order = order.ToString()
            }, withHighlighting, anyAggregation); ;
        }
        public DotaceSearchResult SimpleSearch(DotaceSearchResult search,
            bool withHighlighting = false,
            AggregationContainerDescriptor<Dotace> anyAggregation = null)
        {
            
            var page = search.Page - 1 < 0 ? 0 : search.Page - 1;

            var sw = new StopWatchEx();
            sw.Start();
            search.OrigQuery = search.Q;
            search.Q = Lib.Search.Tools.FixInvalidQuery(search.Q ?? "", queryShorcuts, queryOperators);

            ISearchResponse<Dotace> res = null;
            try
            {
                res = _esClient
                        .Search<Dotace>(s => s
                        .Size(search.PageSize)
                        .ExpandWildcards(Elasticsearch.Net.ExpandWildcards.All)
                        .From(page * search.PageSize)
                        .Query(q => GetSimpleQuery(search))
                        .Sort(ss => GetSort(Convert.ToInt32(search.Order)))
                        .Highlight(h => Lib.Search.Tools.GetHighlight<Dotace>(withHighlighting))
                        .Aggregations(aggr => anyAggregation)
                );
            }
            catch (Exception e)
            {
                if (res != null && res.ServerError != null)
                {
                    Manager.LogQueryError<Dotace>(res, "Exception, Orig query:"
                        + search.OrigQuery + "   query:"
                        + search.Q
                        + "\n\n res:" + search.Result.ToString()
                        , ex: e);
                }
                else
                {
                    HlidacStatu.Util.Consts.Logger.Error("", e);
                }
                throw;
            }
            sw.Stop();

            if (res.IsValid == false)
            {
                Manager.LogQueryError<Dotace>(res, "Exception, Orig query:"
                    + search.OrigQuery + "   query:"
                    + search.Q
                    + "\n\n res:" + search.Result?.ToString()
                    );
            }

            search.Total = res?.Total ?? 0;
            search.IsValid = res?.IsValid ?? false;
            search.ElasticResults = res;
            search.ElapsedTime = sw.Elapsed;
            return search;
        }

        public SortDescriptor<Dotace> GetSort(int iorder)
        {
            DotaceSearchResult.DotaceOrderResult order = (DotaceSearchResult.DotaceOrderResult)iorder;

            SortDescriptor<Dotace> s = new SortDescriptor<Dotace>().Field(f => f.Field("_score").Descending());
            switch (order)
            {
                case DotaceSearchResult.DotaceOrderResult.DateAddedDesc:
                    s = new SortDescriptor<Dotace>().Field(m => m.Field(f => f.PodpisDatum).Descending());
                    break;
                case DotaceSearchResult.DotaceOrderResult.DateAddedAsc:
                    s = new SortDescriptor<Dotace>().Field(m => m.Field(f => f.PodpisDatum).Ascending());
                    break;
                case DotaceSearchResult.DotaceOrderResult.LatestUpdateDesc:
                    s = new SortDescriptor<Dotace>().Field(m => m.Field(f => f.DTAktualizace).Descending());
                    break;
                case DotaceSearchResult.DotaceOrderResult.LatestUpdateAsc:
                    s = new SortDescriptor<Dotace>().Field(m => m.Field(f => f.DTAktualizace).Ascending());
                    break;
                case DotaceSearchResult.DotaceOrderResult.FastestForScroll:
                    s = new SortDescriptor<Dotace>().Field(f => f.Field("_doc"));
                    break;
                case DotaceSearchResult.DotaceOrderResult.Relevance:
                default:
                    break;
            }

            return s;

        }
    }
}
