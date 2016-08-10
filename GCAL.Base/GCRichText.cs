using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCRichText
    {
        public class TextItem
        {
        }

        public class Text
        {
            public bool Italic = false;
            public bool Bold = false;
            public double TextSize = -1.0;
            public string Value = string.Empty;
        }

        public class TextLine : Text
        {
        }

        public class H1: TextLine
        {
        }

        public class H1Subtitle : TextLine
        {
        }

        public class Lines : List<TextItem>
        {
        }

        public class Space : TextItem
        {
            public double SpaceHeight = 0.0;
        }

        public class TableCell
        {
            public Lines Lines = new Lines();
        }

        public class TableRow
        {
            public List<TableCell> Cells = new List<TableCell>();
        }

        public class Table: TextItem
        {
            public List<TableRow> Rows = new List<TableRow>();
        }

        public List<TextItem> TextLines = new List<TextItem>();
    }
}
