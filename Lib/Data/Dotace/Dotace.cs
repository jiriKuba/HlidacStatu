﻿using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HlidacStatu.Lib.Data.Dotace
{
    [ElasticsearchType(IdProperty = nameof(IdDotace))]
    public partial class Dotace : Bookmark.IBookmarkable
    {
        [Nest.Keyword]
        public string IdDotace { get; set; }
        [Nest.Date]
        public DateTime? DatumPodpisu { get; set; }
        [Nest.Keyword]
        public string IdProjektu { get; set; }
        [Nest.Keyword]
        public string KodProjektu { get; set; }
        [Nest.Text]
        public string NazevProjektu { get; set; }

        // calculated fields
        // Celková výše dotace včetně půjčky
        [Nest.Number]
        public float DotaceCelkem { get; set; }
        // Půjčené peníze
        [Nest.Number]
        public float PujckaCelkem { get; set; }
        [Nest.Date]
        public DateTime? DatumAktualizace { get; set; }
        [Nest.Keyword]
        public string UrlZdroje { get; set; }
        [Nest.Keyword]
        public string NazevZdroje { get; set; }

        [Nest.Object]
        public List<Rozhodnuti> Rozhodnuti { get; set; }

        [Nest.Object]
        public Prijemce Prijemce { get; set; }

        [Nest.Object]
        public DotacniProgram DotacniProgram { get; set; }

        public string BookmarkName()
        {
            return this.NazevProjektu;
        }

        public string GetNazevDotace()
        {
            if (!string.IsNullOrWhiteSpace(NazevProjektu))
            {
                return NazevProjektu;
            }
            if (!string.IsNullOrWhiteSpace(IdProjektu))
            {
                return IdProjektu;
            }
            if (!string.IsNullOrWhiteSpace(KodProjektu))
            {
                return KodProjektu;
            }

            return "";
        }

        public string GetUrl(bool local = true)
        {
            return GetUrl(local, string.Empty);
        }

        public string GetUrl(bool local, string foundWithQuery)
        {
            string url = "/Dotace/Detail/" + this.IdDotace;
            if (!string.IsNullOrEmpty(foundWithQuery))
                url = url + "?qs=" + System.Net.WebUtility.UrlEncode(foundWithQuery);

            if (local == false)
                return "https://www.hlidacstatu.cz" + url;
            else
                return url;
        }

        public void Save()
        {
            //calculate fields before saving
            CalculateTotals();
            var res = ES.Manager.GetESClient_Dotace().Index<Dotace>(this, o => o.Id(this.IdDotace)); //druhy parametr musi byt pole, ktere je unikatni
            if (!res.IsValid)
            {
                throw new ApplicationException(res.ServerError?.ToString());
            }
        }

        public string ToAuditJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public string ToAuditObjectId()
        {
            return this.IdDotace;
        }

        public string ToAuditObjectTypeName()
        {
            return "Dotace";
        }


        private void CalculateTotals()
        {
            DotaceCelkem = Rozhodnuti
                .Sum(p => p.CastkaRozhodnuta);
            PujckaCelkem = Rozhodnuti
                .Where(p => p.JePujcka)
                .Sum(p => p.CastkaRozhodnuta);
        }

    }
}
