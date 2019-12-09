﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HlidacStatu.Lib.Search.Rules
{
    public class SimpleValueReplace
        : RuleBase
    {

        string _valueConstrain = "";
        public SimpleValueReplace(string replaceWith, string valueConstrain, bool stopFurtherProcessing = false, string addLastCondition = "")
            : base(replaceWith, stopFurtherProcessing, addLastCondition)
        {
            this._valueConstrain = valueConstrain;
        }


        protected override RuleResult processQueryPart(SplittingQuery.Part part)
        {
            if (this.ReplaceWith.Contains("${q}")
                && (
                    string.IsNullOrWhiteSpace(_valueConstrain) 
                    || Regex.IsMatch(part.Value, part.ToQueryString, HlidacStatu.Util.Consts.DefaultRegexQueryOption)
                    )
                )
            {
                string rq = " " + ReplaceWith.Replace("${q}", part.Value);
                return new RuleResult(SplittingQuery.SplitQuery($" {rq} "), this.StopFurtherProcessing);
            }

            return new RuleResult(part, this.StopFurtherProcessing);
        }

    }
}
