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
        if (!model.Value || model.Value == null || model.Value == '')
            model.Value = '0';
        else {
            model.OriginalValue = parseFloat(model.Value);
        }

        if (model.parentId && model.parentId > 0) {
            var parentItem = treeList.dataSource.get(model.parentId);
            if (model.id == 0) {
                model.set("OriginalValue", model.OriginalValue);
                model.set("Value", model.Value);
                parentItem.Children.push(model);
                parentItem.set("Children", parentItem.Children);
            }

            updateParent(parentItem);
        }
        treeList.dataSource.sync();
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

function getChildrenSum(parentItem) {
    var sum = 0;

    if (parentItem.Children) {
        parentItem.Children.forEach(function (child) {
            sum += child.OriginalValue;
            sum += getChildrenSum(child);
        });
    }

    return sum;
}

function onRemove(e) {
    var treeList = $("#treeList").data("kendoTreeList");
    var dataItem = e.model;
    treeList.dataSource.remove(dataItem);
    treeList.dataSource.sync();
}

function onDataBound(e) {
    var items = e.sender.items();
    for (var i = 0; i < items.length; i++) {
        var row = $(items[i]);
        var dataItem = e.sender.dataItem(row);

        row.find(".value-column").attr("title", dataItem.OriginalValue);

        if (!dataItem.isNew()) {
            if (dataItem.Children && dataItem.Children.length > 0) {
                row.find(".value-column").text(dataItem.Value);

                if (dataItem.Value > 1000) {
                    row.find(".value-column").css("font-weight", "bold");
                }
            }
            else {
                row.find(".value-column").text(formatValue(dataItem.OriginalValue));

                if (dataItem.OriginalValue > 1000) {
                    row.find(".value-column").css("font-weight", "bold");
                }
            }
        }

        if (dataItem.isNew()) {
            row.find("[data-command='createchild']").hide();
        } else {
            row.find("[data-command='createchild']").show();
        }
    }
}

function formatValue(value) {
    if (value > 0) {
        return value % 1 === 0 ? value.toFixed(0) : value.toFixed(2);
    }
    return value;
}

function isNameEditable(e) {
    return true;
}

function isValueEditable(e) {
    return true;
}