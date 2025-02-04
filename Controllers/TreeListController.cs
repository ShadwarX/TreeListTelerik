using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TelerikAspNetCoreApp1.Controllers
{
    public class TreeListController : Controller
    {
        private const string SessionKey = "TreeListData";

        private Dictionary<int, TreeListModel> GetSessionData()
        {
            var data = HttpContext.Session.GetString(SessionKey);

            return data != null ? JsonConvert.DeserializeObject<Dictionary<int, TreeListModel>>(data) : new Dictionary<int, TreeListModel>();
        }

        private void SaveSessionData(Dictionary<int, TreeListModel> data)
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
            return Json(data.Values.ToTreeDataSourceResult(request, e => e.Id, e => e.TreeListModelId, e => e));
        }

        public JsonResult Create([DataSourceRequest] DataSourceRequest request, TreeListModel treeModel)
        {
            if (treeModel == null) return null;

            var data = GetSessionData();
            treeModel.Id = data.Count > 0 ? data.Keys.Max(x => x) + 1 : 1;
            data[treeModel.Id] = treeModel;

            if (double.TryParse(treeModel.Value.Replace('.', ','), out var valueTree) && double.TryParse(treeModel.OriginalValue.Replace('.', ','), out valueTree))
            {
                if (treeModel.TreeListModelId > 0)
                {
                    if (treeModel.TreeListModelId != null && data.TryGetValue((int)treeModel.TreeListModelId, out var parent))
                    {
                        parent.Visited = false;
                        parent.Children.Add(treeModel.Id);
                        UpdateParents(data, parent.Id);
                    }
                }
            }

            SaveSessionData(data);

            return Json(new[] { treeModel }.ToTreeDataSourceResult(request, ModelState));
        }

        public JsonResult Destroy([DataSourceRequest] DataSourceRequest request, TreeListModel treeModel)
        {
            var data = GetSessionData();
            if (TryRemoveTreeNode(data, treeModel.Id))
                SaveSessionData(data);
            return Json(new[] { treeModel }.ToTreeDataSourceResult(request, ModelState));
        }

        private bool TryRemoveTreeNode(Dictionary<int, TreeListModel> data, int id)
        {
            if (data.TryGetValue(id, out var treeItem))
            {
                data.Remove(treeItem.Id);
                if (treeItem.TreeListModelId != null && treeItem.TreeListModelId > 0 && data.TryGetValue((int)treeItem.TreeListModelId, out var parent))
                    parent.Children?.Remove(treeItem.Id);
                if (treeItem.TreeListModelId != null)
                    UpdateParents(data, (int)treeItem.TreeListModelId);

                if (treeItem.Children.Count > 0)
                {
                    foreach (var child in treeItem.Children)
                        TryRemoveTreeNode(data, child);
                }

                return true;
            }

            return false;
        }

        public JsonResult Update([DataSourceRequest] DataSourceRequest request, TreeListModel treeModel)
        {
            var data = GetSessionData();
            if (data.TryGetValue(treeModel.Id, out var treeItem))
            {
                treeItem.Name = treeModel.Name;

                if ((treeModel.Value != treeItem.Value || treeItem.OriginalValue != treeModel.OriginalValue)
                    && double.TryParse(treeModel.Value.Replace('.', ','), out var originalValue) && double.TryParse(treeModel.OriginalValue.Replace('.', ','), out originalValue))
                {
                    treeItem.Visited = false;
                    treeItem.Value = treeModel.Value;
                    treeItem.OriginalValue = treeModel.OriginalValue;

                    UpdateParents(data, treeItem.Id);
                    SaveSessionData(data);
                }
            }
            return Json(new[] { treeItem }.ToTreeDataSourceResult(request, ModelState));
        }

        private void UpdateParents(Dictionary<int, TreeListModel> data, int parentId)
        {
            if (data.TryGetValue(parentId, out var parent))
            {
                if (parent.Children.Count == 0)
                    parent.Value = parent.OriginalValue.Replace(',', '.');
                else
                    parent.Value = CalculateSum(data, parent.Children, parent.Id).ToString().Replace(',', '.');
                parent.Visited = true;

                if (parent.TreeListModelId != null)
                    UpdateParents(data, (int)parent.TreeListModelId);
            }
        }

        private double CalculateSum(Dictionary<int, TreeListModel> data, List<int> itemsId, int id)
        {
            double total = 0.0d;

            foreach (var itemId in itemsId)
            {
                if (data.TryGetValue(itemId, out var treeItem))
                {
                    if (treeItem.Visited)
                    {
                        total += double.Parse(treeItem.Value.Replace('.', ','));
                        if (treeItem.Children.Count > 0)
                            total += double.Parse(treeItem.OriginalValue.Replace('.', ','));
                    }
                    else
                    {
                        double current = 0.0d;
                        if (treeItem.Children.Count > 0)
                        {
                            current = CalculateSum(data, treeItem.Children, treeItem.Id);
                            treeItem.Value = current.ToString().Replace(',', '.');
                        }
                        else
                        {
                            treeItem.Value = treeItem.OriginalValue.Replace(',', '.');
                        }
                        treeItem.Visited = true;
                        total += current + double.Parse(treeItem.OriginalValue.Replace('.', ','));
                    }
                }
            }

            return total;
        }
    }
}