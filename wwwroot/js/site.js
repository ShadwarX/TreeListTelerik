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

    if (model.Name && model.Name.trim() !== "" && model.Value !== undefined && model.Value !== null && model.Value != '') {
        treeList.dataSource.sync();
    }
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
    return e.Id == 0;
}