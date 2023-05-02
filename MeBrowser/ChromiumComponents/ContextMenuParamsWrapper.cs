using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.ChromiumComponents
{
    public class ContextMenuParamsWrapper
    {
        /// <summary>
        /// Dictioanry Suggestions
        /// </summary>
        public IList<string> DictionarySuggestions { get; private set; }
        /// <summary>
        /// X Coordinate
        /// </summary>
        public int XCoord { get; private set; }
        /// <summary>
        /// Y Coordinate
        /// </summary>
        public int YCoord { get; private set; }
        /// <summary>
        /// Selection Text
        /// </summary>
        public string SelectionText { get; private set; }
        /// <summary>
        /// Misspelled Word
        /// </summary>
        public string MisspelledWord { get; private set; }

        public string LinkUrl { get; private set; }
        public string PageUrl { get; private set; }
        public string SourceUrl { get; private set; }

        public ContextMenuParamsWrapper(IList<string> dictionarySuggestions, int xCoord, int yCoord, string selectionText, string misspelledWord, string linkUrl, string pageUrl, string sourceUrl)
        {
            DictionarySuggestions = dictionarySuggestions;
            XCoord = xCoord;
            YCoord = yCoord;
            SelectionText = selectionText;
            MisspelledWord = misspelledWord;
            LinkUrl = linkUrl;
            PageUrl = pageUrl;
            SourceUrl = sourceUrl;
        }
    }
}
