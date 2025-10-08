using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OVNMPR.UserControl.Helpers
{
    internal class GridViewMenuHelper
    {
        private readonly BarManager barManager;
        private readonly PopupMenu popupMenu;

        public GridViewMenuHelper(Form parentForm)
        {
            barManager = new BarManager();
            barManager.Form = parentForm;

            popupMenu = new PopupMenu(barManager);
        }

        /// <summary>
        /// Tạo context menu cho GridView.
        /// </summary>
        /// <param name="gridView">GridView cần gắn menu.</param>
        /// <param name="menuItems">Dictionary: Key = tên menu, Value = action callback.</param>
        public void CreateMenu(GridView gridView, Dictionary<string, Action<GridView>> menuItems)
        {
            foreach (var kvp in menuItems)
            {
                var item = new BarButtonItem(barManager, kvp.Key);
                popupMenu.AddItem(item);

                // Gắn sự kiện click
                item.ItemClick += (s, e) => kvp.Value(gridView);
            }

            // Bắt sự kiện chuột phải
            gridView.PopupMenuShowing += (s, e) =>
            {
                if (e.HitInfo.InRow || e.HitInfo.InGroupRow)
                {
                    gridView.FocusedRowHandle = e.HitInfo.RowHandle;
                    popupMenu.ShowPopup(Control.MousePosition);
                }
            };
        }
    }
}
