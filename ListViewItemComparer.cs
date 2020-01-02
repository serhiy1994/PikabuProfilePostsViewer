using System;
using System.Windows.Forms;
using System.Collections;

namespace PikabuProfilePostsViewer
{
    class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder order;
        public ListViewItemComparer()
        {
            col = 0;
            order = SortOrder.Ascending;
        }
        public ListViewItemComparer(int column, SortOrder order)
        {
            col = column;
            this.order = order;
        }
        public int Compare(object x, object y)
        {
            int returnVal;
            try
            {
                System.DateTime firstDate = DateTime.Parse(((ListViewItem)x).SubItems[col].Text);
                System.DateTime secondDate = DateTime.Parse(((ListViewItem)y).SubItems[col].Text);
                returnVal = DateTime.Compare(firstDate, secondDate);
            }
            catch
            {
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            }
            if (order == SortOrder.Descending)
                returnVal *= -1;
            return returnVal;
        }
    }
}
