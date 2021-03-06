﻿using System;
using System.Collections.Generic;
using System.Linq;
using Devmasters.Core;
using Devmasters.Core.Collections;

namespace HlidacStatu.Util
{
    public class InfoFact
    {
        public InfoFact() { }
        public InfoFact(string txt, ImportanceLevel level)
        {
            this.Text = txt;
            this.Level = level;
        }

        public string Text { get; set; }
        public ImportanceLevel Level { get; set; }

        public enum ImportanceLevel
        {
            Summary = 100,
            Stat = 70,
            High = 50,
            Medium = 25,
            Low = 10,
            NotAtAll = 1
        }

        public string Render(bool html = true)
        {
            if (html)
                return Text;
            else
                return Devmasters.Core.TextUtil.RemoveHTML(Text);
        }

        public static string RenderInfoFacts(InfoFact[] infofacts, int number,
            bool takeSummary = true, bool shuffle = false,
            string delimiterBetween = " ",
            string lineFormat = "{0}", bool html = false)
        {
            if ((infofacts?.Count() ?? 0) == 0)
                return string.Empty;

            infofacts = infofacts.OrderByDescending(o => o.Level).ToArray();

            var taken = new List<InfoFact>();
            if (takeSummary)
                taken.Add(infofacts.First());


            if (number > 1)
            {
                if (shuffle)
                    taken.AddRange(infofacts.Skip(takeSummary ? 1 : 0).Shuffle().Take(number - (takeSummary ? 1 : 0)));
                else
                    taken.AddRange(infofacts.Skip(takeSummary ? 1 : 0).Take(number - (takeSummary ? 1 : 0)));
            }

            bool useStringF = lineFormat.Contains("{0}");

            return taken
                .Select(t => useStringF ? string.Format(lineFormat, t.Render(html)) : t.Render(html))
                .Aggregate((f, s) => f + delimiterBetween + s);
        }

    }
}
