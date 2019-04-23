using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.ProductViewModels
{
    public class ProductCreateViewModel
    {
        public Product Product { get; set; }

        public List<SelectListItem> UnalteredList { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> SelectionList
        {
            get
            {
                List<SelectListItem> SelectionList = UnalteredList;
                SelectionList.Insert(0, new SelectListItem
                {
                    Value = null,
                    Text = "Select a Category...",
                    Selected = true
                });
                return SelectionList;

            }
        }
    }
}
