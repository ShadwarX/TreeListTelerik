using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telerik.SvgIcons;

namespace TelerikAspNetCoreApp1.Controllers
{
    public class TreeListController : Controller
    {
        private const string SessionKey = "TreeListData";

        private List<TreeListModel> GetSessionData()
        {
            var data = HttpContext.Session.GetString(SessionKey);

            return data != null ? JsonConvert.DeserializeObject<List<TreeListModel>>(data) : new List<TreeListModel>();
        }

        private void SaveSessionData(List<TreeListModel> data)
        {
            HttpContext.Session.SetString(SessionKey, JsonConvert.SerializeObject(data));
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult GetAll([DataSourceRequest] DataSourceRequest request)
        {
            var data = GetSessionData();
            return Json(data.ToTreeDataSourceResult(request, e => e.Id, e => e.TreeListModelId, e => e));
        }

        public JsonResult Create([DataSourceRequest] DataSourceRequest request, TreeListModel treeModel)
        {
            if (treeModel == null) return null;

            var data = GetSessionData();
            treeModel.Id = data.Count > 0 ? data.Max(x => x.Id) + 1 : 1;
            data.Add(treeModel);
            if (treeModel.TreeListModelId > 0)
                data.FirstOrDefault(x => x.Id == treeModel.TreeListModelId)?.Children.Add(treeModel);

            if (!string.IsNullOrEmpty(treeModel.Value))
                treeModel.Value = treeModel.Value.Replace('.', ',');
            if (double.TryParse(treeModel.Value, out var originalValue))
            {
                treeModel.OriginalValue = originalValue;
                SaveSessionData(data);
                UpdateValues(data);
            }

            return Json(new[] { treeModel }.ToTreeDataSourceResult(request, ModelState));
        }

        public JsonResult Destroy([DataSourceRequest] DataSourceRequest request, TreeListModel treeModel)
        {
            var data = GetSessionData();
            var item = data.FirstOrDefault(e => e.Id == treeModel.Id);
            if (item != null)
            {
                data.Remove(item);
                data.FirstOrDefault(x => x.Id == item.TreeListModelId)?.Children.Remove(item);
                data.RemoveAll(e => e.TreeListModelId == treeModel.Id);
            }
            SaveSessionData(data);
            UpdateValues(data);
            return Json(new[] { treeModel }.ToTreeDataSourceResult(request, ModelState));
        }

        public JsonResult Update([DataSourceRequest] DataSourceRequest request, TreeListModel treeModel)
        {
            var data = GetSessionData();
            var model = data.FirstOrDefault(e => e.Id == treeModel.Id);
            if (model != null)
            {
                model.Name = treeModel.Name;
                if (treeModel.Value != model.OldValue)
                {
                    if (double.TryParse(treeModel.Value, out var originalValue))
                    {
                        model.OriginalValue = originalValue;
                        SaveSessionData(data);
                        UpdateValues(data);
                    }
                }
            }
            return Json(new[] { model }.ToTreeDataSourceResult(request, ModelState));
        }

        private void UpdateValues(List<TreeListModel> data)
        {
            foreach (var item in data)
            {
                var sum = CalculateSum(data, item.Id);

                if (sum == 0)
                    item.Value = "0";
                else
                {
                    item.OldValue = item.Value;
                    item.Value = sum.ToString();
                }
            }
            SaveSessionData(data);
        }

        private double CalculateSum(List<TreeListModel> data, int id)
        {
            var item = data.FirstOrDefault(e => e.Id == id);
            if (item == null) return 0;

            double total = 0.0d;
            foreach (var other in data.Where(e => e.TreeListModelId == id))
            {
                total += other.OriginalValue;
                total += CalculateSum(data, other.Id);
            }

            return total;
        }
    }
}