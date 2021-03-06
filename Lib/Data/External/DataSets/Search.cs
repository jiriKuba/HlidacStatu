﻿using HlidacStatu.Lib.Searching.Rules;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HlidacStatu.Lib.Data.External.DataSets
{
    public static class Search
    {

        public static string GetSpecificQueriesForDataset(DataSet ds, string mappingProperty, string query, bool addKeyword, string attrNameModif = "")
        {
            var props = ds.GetMappingList(mappingProperty, attrNameModif).ToArray();
            if (addKeyword)
                for (int i = 0; i < props.Length; i++)
                {
                    props[i] += ".keyword";
                }
            var qSearch = SearchDataQuery(props, query);
            return qSearch;
        }

        public static Dictionary<DataSet, string> GetSpecificQueriesForDatasets(string mappingProperty, string query, bool addKeyword)
        {
            Dictionary<DataSet, string> queries = new Dictionary<DataSet, string>();
            foreach (var ds in DataSetDB.ProductionDataSets.Get())
            {
                var qSearch = GetSpecificQueriesForDataset(ds, mappingProperty, query, addKeyword);
                if (!string.IsNullOrEmpty(qSearch))
                {
                    queries.Add(ds, qSearch);
                }
            }

            return queries;
        }

        static string[] queryOperators = new string[] {
            "AND","OR"
        };
        static string[] queryShorcuts = new string[] {
                "ico:",
                "osobaid:",
                "holding:",
                "gps_lat:",
                "gps_lng:",
                "id:",
        };

        public static string SearchDataQuery(string[] properties, string value)
        {
            if (properties == null)
                return string.Empty;
            if (properties.Count() == 0)
                return string.Empty;
            //create query
            string q = properties
                .Select(p => $"{p}:{value}")
                .Aggregate((f, s) => f + " OR " + s);
            return $"( {q} )";
        }
        public static DataSearchResult SearchData(DataSet ds, string queryString, int page, int pageSize, 
            string sort = null, bool excludeBigProperties = true, bool withHighlighting = false,
            bool exactNumOfResults = false)
        {
            Devmasters.Core.StopWatchEx sw = new Devmasters.Core.StopWatchEx();

            sw.Start();
            var query = Lib.Searching.Tools.FixInvalidQuery(queryString, queryShorcuts, queryOperators);

            var res = _searchData(ds, query, page, pageSize, sort, excludeBigProperties, withHighlighting, exactNumOfResults);

            sw.Stop();
            if (!res.IsValid)
            {
                throw DataSetException.GetExc(
                    ds.DatasetId,
                    ApiResponseStatus.InvalidSearchQuery.error.number,
                    ApiResponseStatus.InvalidSearchQuery.error.description,
                    queryString
                    );
            }

            if (res.Total > 0)
                return new DataSearchResult()
                {
                    ElapsedTime = sw.Elapsed,
                    Q = queryString,
                    IsValid = true,
                    Total = res.Total,
                    Result = res.Hits
                            .Select(m => Newtonsoft.Json.JsonConvert.SerializeObject(m.Source))
                            .Select(s => (dynamic)Newtonsoft.Json.Linq.JObject.Parse(s)),

                    Page = page,
                    PageSize = pageSize,
                    DataSet = ds,
                    ElasticResultsRaw = res,
                };
            else
                return new DataSearchResult()
                {
                    ElapsedTime = sw.Elapsed,
                    Q = queryString,
                    IsValid = true,
                    Total = 0,
                    Result = new dynamic[] { },
                    Page = page,
                    PageSize = pageSize,
                    DataSet = ds,
                    ElasticResultsRaw = res,

                };
        }
        public static DataSearchRawResult SearchDataRaw(DataSet ds, string queryString, int page, int pageSize, 
            string sort = null, bool excludeBigProperties = true, bool withHighlighting = false,
            bool exactNumOfResults = false)
        {
            var query = Lib.Searching.Tools.FixInvalidQuery(queryString, queryShorcuts, queryOperators);
            var res = _searchData(ds, query, page, pageSize, sort, excludeBigProperties, withHighlighting, exactNumOfResults);
            if (!res.IsValid)
            {
                throw DataSetException.GetExc(ds.DatasetId,
                    ApiResponseStatus.InvalidSearchQuery.error.number,
                    ApiResponseStatus.InvalidSearchQuery.error.description,
                    queryString
                    );
            }

            if (res.Total > 0)
                return new DataSearchRawResult()
                {
                    Q = queryString,
                    IsValid = true,
                    Total = res.Total,
                    Result = res.Hits.Select(m => new Tuple<string, string>(m.Id, Newtonsoft.Json.JsonConvert.SerializeObject(m.Source))),
                    Page = page,
                    PageSize = pageSize,
                    DataSet = ds,
                    ElasticResultsRaw = res,
                    Order = sort ?? "0"
                };
            else
                return new DataSearchRawResult()
                {
                    Q = queryString,
                    IsValid = true,
                    Total = 0,
                    Result = new List<Tuple<string, string>>(),
                    ElasticResultsRaw = res,
                    Page = page,
                    PageSize = pageSize,
                    DataSet = ds,
                    Order = sort ?? "0"
                };
        }

        public static ISearchResponse<object> _searchData(DataSet ds, string queryString, int page, int pageSize, string sort = null, 
            bool excludeBigProperties = true, bool withHighlighting = false, bool exactNumOfResults = false)
        {
            SortDescriptor<object> sortD = new SortDescriptor<object>();
            if (sort == "0")
                sort = null;

            if (!string.IsNullOrEmpty(sort))
            {
                if (sort.EndsWith(DataSearchResult.OrderDesc) || sort.ToLower().EndsWith(DataSearchResult.OrderDescUrl))
                {
                    sort = sort.Replace(DataSearchResult.OrderDesc, "").Replace(DataSearchResult.OrderDescUrl, "").Trim();
                    sortD = sortD.Field(sort, SortOrder.Descending);
                }
                else
                {
                    sort = sort.Replace(DataSearchResult.OrderAsc, "").Replace(DataSearchResult.OrderAscUrl, "").Trim();
                    sortD = sortD.Field(sort, SortOrder.Ascending);
                }
            }


            Nest.ElasticClient client = Lib.ES.Manager.GetESClient(ds.DatasetId, idxType: ES.Manager.IndexType.DataSource);

            QueryContainer qc = GetSimpleQuery(ds, queryString);

            //QueryContainer qc = null;
            //if (queryString == null)
            //    qc = new QueryContainerDescriptor<object>().MatchNone();
            //else if (string.IsNullOrEmpty(queryString))
            //    qc = new QueryContainerDescriptor<object>().MatchAll();
            //else
            //{
            //    qc = new QueryContainerDescriptor<object>()
            //        .QueryString(qs => qs
            //            .Query(queryString)
            //            .DefaultOperator(Operator.And)
            //        );
            //}

            page = page - 1;
            if (page < 0)
                page = 0;
            if (page * pageSize > Lib.Data.Smlouva.Search.MaxResultWindow)
            {
                page = (Lib.Data.Smlouva.Search.MaxResultWindow / pageSize) - 1;
            }

            //exclude big properties from result
            var maps = excludeBigProperties ? ds.GetMappingList("DocumentPlainText").ToArray() : new string[] { };



            var res = client
                .Search<object>(s => s
                    .Size(pageSize)
                    .Source(ss => ss.Excludes(ex => ex.Fields(maps)))
                    .From(page * pageSize)
                    .Query(q => qc)
                    .Sort(ss => sortD)
                    .Highlight(h => Lib.Searching.Tools.GetHighlight<Object>(withHighlighting))
                    .TrackTotalHits(exactNumOfResults ? true : (bool?)null)
           );

            //fix Highlighting for large texts
            if (withHighlighting 
                && res.Shards != null 
                && res.Shards.Failed > 0) //if some error, do it again without highlighting
            {
                res = client
                    .Search<object>(s => s
                        .Size(pageSize)
                        .Source(ss => ss.Excludes(ex => ex.Fields(maps)))
                        .From(page * pageSize)
                        .Query(q => qc)
                        .Sort(ss => sortD)
                        .Highlight(h => Lib.Searching.Tools.GetHighlight<Object>(false))
                        .TrackTotalHits(exactNumOfResults ? true : (bool?)null)
                );

            }

            Audit.Add(Audit.Operations.Search, "", "", "Dataset."+ds.DatasetId, res.IsValid ? "valid" : "invalid", queryString, null);

            return res;
        }


        static string simpleQueryOsobaPrefix = "___";
        //static RegexOptions regexQueryOption = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline;
        public static QueryContainer GetSimpleQuery(DataSet ds, string query)
        {
            var idQuerypath = GetSpecificQueriesForDataset(ds, "id", "${q}", false);

            var icoQuerypath = GetSpecificQueriesForDataset(ds, "ICO", "${q}", false);
            //var osobaIdQuerypathToIco = GetSpecificQueriesForDataset(ds, "OsobaId", "${q}", true)
            //                + " OR ( " + simpleQueryOsobaPrefix + "osobaid" + simpleQueryOsobaPrefix + ":${v} )";

            var osobaIdQuerypath = GetSpecificQueriesForDataset(ds, "OsobaId", "${q}", true);

            //var osobaQP = "";
            //if (!string.IsNullOrEmpty(icoQuerypath) && !string.IsNullOrEmpty(osobaIdQuerypathToIco))
            //    osobaQP = $"({icoQuerypath} OR {osobaIdQuerypathToIco})";
            //else if (!string.IsNullOrEmpty(icoQuerypath))
            //    osobaQP = icoQuerypath;
            //else if (!string.IsNullOrEmpty(osobaIdQuerypathToIco))
            //    osobaQP = osobaIdQuerypathToIco;

            List<Lib.Searching.Rule> rules = new List<Lib.Searching.Rule> {
                    new Lib.Searching.Rule("id:",idQuerypath ),
                    new Lib.Searching.Rule(@"osobaid:(?<q>((\w{1,} [-]{1} \w{1,})([-]{1} \d{1,3})?)) ", "ico")
                    {
                        AddLastCondition = osobaIdQuerypath
                    },
                    new Lib.Searching.Rule(@"holding:(?<q>(\d{1,8})) ",icoQuerypath ),
                    new Lib.Searching.Rule("ico:",icoQuerypath ),
                    new Lib.Searching.Rule(simpleQueryOsobaPrefix+@"osobaid" + simpleQueryOsobaPrefix + @":(?<q>((\w{1,} [-]{1} \w{1,})([-]{1} \d{1,3})?)) ", osobaIdQuerypath,false),
                };

            List<IRule>irules = new List<IRule> {
               new TransformPrefixWithValue("id:",idQuerypath,null ),
               new OsobaId("osobaid:",icoQuerypath,addLastCondition:osobaIdQuerypath ),
               new Holding(null,icoQuerypath ),
               //new TransformPrefix("osobaid","",null,false),

               new TransformPrefixWithValue("ico:",icoQuerypath,null ),
            };


            // add searched prefixes
            string[] existingPrefixes = irules.
                SelectMany(m => m.Prefixes)
                .Where(m => !string.IsNullOrEmpty(m))
                .ToArray();

            var qp = HlidacStatu.Lib.Searching.SplittingQuery.SplitQuery(query);
            string[] foundPrefixes = qp.Parts.Select(m=>m.Prefix)
                .Where(m => !string.IsNullOrEmpty(m))
                .Where(m => !existingPrefixes.Contains(m))
                .ToArray();

            foreach (var fPref in foundPrefixes)
            {
                var pref = fPref.Substring(0, fPref.Length - 1);
                string qpref = GetSpecificQueriesForDataset(ds, pref, "${q}", false);
                string qpref_keyw = GetSpecificQueriesForDataset(ds, pref, "${q}", true);
                string prefPath = "";
                if (!string.IsNullOrWhiteSpace(qpref)
                    && !string.IsNullOrWhiteSpace(qpref_keyw)
                    )
                    prefPath = $"( {qpref} OR {qpref_keyw} )";
                else if (!string.IsNullOrWhiteSpace(qpref)
                    && string.IsNullOrWhiteSpace(qpref_keyw))
                    prefPath = $" {qpref} ";
                else if (!string.IsNullOrWhiteSpace(qpref_keyw)
                    && string.IsNullOrWhiteSpace(qpref))
                    prefPath = $" {qpref_keyw} ";

                if (!string.IsNullOrWhiteSpace(prefPath))
                {
                    //rules.Add(new Lib.Search.Rule(fPref, prefPath));
                    irules.Add(new TransformPrefixWithValue(fPref, prefPath, null));
                }
            }


            //var qc = Lib.Search.Tools.GetSimpleQuery<object>(query, rules.ToArray());

            var qc = Lib.Searching.SimpleQueryCreator.GetSimpleQuery<object>(query, irules.ToArray());

            return qc;
        }

    }
}
