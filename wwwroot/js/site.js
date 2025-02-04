var isEditing = false;
function onEdit(e) {

    if (isEditing) {
        e.preventDefault();
        return;
    }

    isEditing = true;
}

function onCellClose(e) {
    var treeList = $("#treeList").data("kendoTreeList");
    var model = e.model;

    if (model.Name && model.Name.trim() !== "") {
        if (!model.Value || model.Value == null || model.Value === '') {
            model.Value = '0';
        }
        model.OriginalValue = model.Value;
        var result = tryParse(model.Value);
        if (!result.success)
            return;
        model.Value = result.value.toString();
        result = tryParse(model.OriginalValue);
        if (!result.success)
            return;
        model.OriginalValue = result.value.toString();

        if (treeList.dataSource) {
            if (model.parentId && model.parentId > 0) {

                if (!model.isNew()) {
                    if (model.Children && model.Children.length > 0) {
                        var sum = getChildrenSum(treeList, model.Children);
                        model.Value = sum.toString();
                    }
                }

                getParentValue(treeList, model);
            }
        }

        treeList.dataSource.sync().then(() => {
            treeList.refresh();
        });
    }
}

function updateParent(parentItem) {
    var treeList = $("#treeList").data("kendoTreeList");

    if (parentItem) {
        var sum = getChildrenSum(parentItem);
        parentItem.set("Value", sum.toString());
        treeList.dataSource.sync();
    }
}

function getParentValue(treeList, children) {
    var parentItem = treeList.dataSource.get(children.parentId);
    if (parentItem) {
        parentItem.Value = getChildrenSum(treeList, parentItem.Children).toString();
        getParentValue(treeList, parentItem);
    }
}

function getChildrenSum(treeList, children) {
    let sum = 0;

    if (children) {
        children.forEach(function (child) {
            var childItem = treeList.dataSource.get(child);
            if (childItem) {
                sum += parseFloat(childItem.OriginalValue);

                if (childItem.Children)
                    sum += getChildrenSum(treeList, childItem.Children);
            }
        });
    }

    return sum;
}

function onRemove(e) {
    var treeList = $("#treeList").data("kendoTreeList");
    var dataItem = e.model;
    if (dataItem.Children && dataItem.Children.length > 0)
        removeChildren(treeList, dataItem.Children);
    treeList.dataSource.remove(dataItem);
    treeList.dataSource.sync().then(() => {
        treeList.refresh();
    });
}

function removeChildren(treeList, children) {
    children.forEach(function (child) {
        var childItem = treeList.dataSource.get(child);
        if (childItem) {
            childItem.parentId = null;
            if (childItem.Children && childItem.Children.length > 0)
                removeChildren(treeList, childItem.Children);
        }
    });
}

function onDataBound(e) {
    var treeList = $("#treeList").data("kendoTreeList");
    var items = e.sender.items();
    for (var i = 0; i < items.length; i++) {
        var row = $(items[i]);
        var dataItem = e.sender.dataItem(row);

        var result = tryParse(dataItem.OriginalValue);
        if (result.success) {

            if (result.value == 0)
                row.find(".value-column").attr("title", 0);
            else
            {
                var newValue = formatValue(dataItem.OriginalValue);
                row.find(".value-column").attr("title", newValue);
            }
        }

        if (dataItem.Value > 1000) {
            row.find(".value-column").css("font-weight", "bold");
        }

        if (dataItem.isNew()) {
            row.find("[data-command='createchild']").hide();
        } else {
            row.find("[data-command='createchild']").show();
        }
    }
}

function tryParse(value) {
    var editValue = value.replace(',', '.');
    const parsed = parseFloat(editValue);
    if (!isNaN(parsed))
    {
        return { success: true, value: parsed % 1 == 0 ? parsed.toFixed(0) : parsed.toFixed(2) };
    }
    else
    {
        return { success: false, value: null };
    }
}

function formatValue(value) {
    var newValue = parseFloat(value.replace(',', '.'));
    if (newValue > 0) {
        return newValue % 1 === 0 ? newValue.toFixed(0) : newValue.toFixed(2);
    }
    return value;
}

function isNameEditable(e) {
    return true;
}

function isValueEditable(e) {
    return true;
}