using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ShapeConverter.BusinessLogic.Parser.Svg.CSS
{
    internal class CssSelector
    {
        private enum XPathSelectorStatus
        {
            Start,
            Parsed,
            Compiled,
            Error
        }

        internal static string SelectorRule =
            @"(?<type>[A-Za-z][A-Za-z0-9]*)?" +
            @"(?<class>\.[A-Za-z][_A-Za-z0-9\-]*)?" +
            @"(?<id>\#[A-Za-z][_A-Za-z0-9\-]*)?" +
            @"(?<combinator>(\s*(\+|\>|\~)\s*)|(\s+))?";


        private int specificity;
        private static Regex reSelector = new Regex(SelectorRule);
        private XPathSelectorStatus Status = XPathSelectorStatus.Start;
        private string selector;
        private string xpath;

        public CssSelector(string selector)
        {
            this.selector = selector.Trim();
            GetXPath();
        }

        public int Specificity
        {
            get
            {
                if (Status == XPathSelectorStatus.Start)
                {
                    GetXPath();
                }

                if (Status != XPathSelectorStatus.Error)
                    return specificity;

                return 0;
            }
        }

        private void AddSpecificity(int a, int b, int c)
        {
            specificity += a * 100 + b * 10 + c;
        }

        private string TypeToXPath(Match match)
        {
            string r = string.Empty;
            Group g = match.Groups["type"];
            string s = g.Value;

            if (g.Success)
            {
                r = " type='" + s + "'";
                AddSpecificity(0, 0, 1);
            }

            return r;
        }

        private string ClassToXPath(Match match)
        {
            string r = string.Empty;
            Group g = match.Groups["class"];
            string s = g.Value;

            if (g.Success)
            {
                AddSpecificity(0, 1, 0);
                r = "class='" + s + "'";
            }
            return r;
        }

        private string IdToXPath(Match match)
        {
            string r = string.Empty;
            Group g = match.Groups["id"];

            if (g.Success)
            {
                r = " id='" + g.Value.Substring(1) + "'";
                AddSpecificity(1, 0, 0);
            }
            return r;
        }

        private string SeperatorToXPath(Match match)
        {
            string r = string.Empty;
            Group g = match.Groups["combinator"];

            if (g.Success)
            {
                string s = g.Value.Trim();

                if (s.Length == 0)
                    s = " ";

                r = " combinator='" + s + "'";
            }

            return r;
        }

        private void GetXPath()
        {
            this.specificity = 0;
            StringBuilder xpath = new StringBuilder("*");

            Match match = reSelector.Match(selector);
            while (match.Success)
            {
                if (match.Success && match.Value.Length > 0)
                {
                    xpath.Append(TypeToXPath(match));
                    xpath.Append(ClassToXPath(match));
                    xpath.Append(IdToXPath(match));
                    xpath.Append(SeperatorToXPath(match));


                }
                match = match.NextMatch();
            }

            Status = XPathSelectorStatus.Parsed;
            this.xpath = xpath.ToString();
        }

        private void Compile()
        {
            if (Status == XPathSelectorStatus.Start)
            {
                GetXPath();
            }
            if (Status == XPathSelectorStatus.Parsed)
            {
                //_xpath = nav.Compile(_sXpath);
                //_xpath.SetContext(GetNSManager());

                Status = XPathSelectorStatus.Compiled;
            }
        }

        //public bool Matches(XPathNavigator nav)
        //{
        //    if (Status != XPathSelectorStatus.Compiled)
        //    {
        //        Compile(nav);
        //    }
        //    if (Status == XPathSelectorStatus.Compiled)
        //    {
        //        try
        //        {
        //            return nav.Matches(_xpath);
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
