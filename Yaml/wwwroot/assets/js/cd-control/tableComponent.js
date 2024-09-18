const tableComponent = (() => {
    const content = '-content';
    const count = {};

    function init(columnSize, selector, buttonNames, colNames, type) {
        $(selector + "-add").off('click')
        $(selector + "-add").off('click').click(function () {
            if (buttonNames && buttonNames.length > 0) {
                addRowWithButton(columnSize, selector, buttonNames, colNames, type)
            } else {
                addRow(columnSize, selector, colNames, type)
            }
        })
    }

    function addRow(columnSize, selector, colNames, type) {

        let tabContent =
            `
            <div class="m-data-table__item" id={{columnId}} rowId="0" colCount={{colCount}}>
                {{content}}
            </div>
            `

        let colItem =
            `
             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                <div class="a-text-field a-text-field--type-text">    
                  <input type="text" name="{{name}}" value="" colname="{{colname}}" class="a-text-field__input" />
                </div>
            </span>
            `

        let buttonColItem =
            `
            <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                <span class="m-data-table__truncate-content">
                    <button class="a-button a-button--text">
                       <button type="button" class="a-add-item-button" onclick="tableComponent.removeRow(this, '${type}')"><i class="a-icon a-icon--close-hover"></i></button> 
                    </button>
                </span>
            </span>
            `

        // assemble table column like :  [colItem | colItem | button]
        let column = ''
        for (let i = 0; i < columnSize - 1; i++) {
            // assemble table item
            column += (colItem
                    .replace("{{name}}", selector + "-" + i)
                    .replace("{{colname}}", colNames[i])
            )
        }
        column += buttonColItem;

        // column auto increment
        if (!count[selector]) {
            count[selector] = 0;
        } else {
            count[selector] = $(selector + content).find(`[colcount]`).length;
        }
        count[selector]++;

        tabContent = tabContent
            .replace("{{colCount}}", count[selector])
            .replace("{{content}}", column)
        if ($(selector + "-btnGroup").length > 0) {
            $(selector + "-btnGroup").before(tabContent);
        } else
            $(selector + "-content").append(tabContent);
    }

    function addRowWithButton(columnSize, selector, buttonNames, colNames, type) {

        let tabContent =
            `
            <div class="m-data-table__item" rowId="0" columnId={{columnId}}>
                {{content}}
            </div>
            `

        let colItem =
            `
             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                <div class="a-text-field a-text-field--type-text" style="max-width: 100px; padding: 7px"> 
                    <input type="file" file-input="{{name}}"  style="display: none" /> 
                    <span class="a-upload-field__description" data-filename ='' selected-file="{{name}}"></span>
                    <button type="button" class="a-button a-button--primary" file-upload-button="{{name}}" colname="{{colname}}"  name="{{name}}" >
                        {{buttonName}}
                    </button>
                </div>
            </span>
            `

        let firstColItem =
            `
             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                <div class="a-text-field a-text-field--type-text">    
                      <input type="text" name="{{name}}" value="" colname="{{colname}}" class="a-text-field__input" />
                </div>
            </span>
            `

        let buttonColItem =
            `
            <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                <span class="m-data-table__truncate-content">
                   <button type="button" class="a-button a-button--text" onclick="tableComponent.removeRow(this, '${type}')">
                     <div class="a-button__label"><i class="a-icon a-icon--close-hover"></i></div>
                   </button> 
                </span>
            </span>
            `
        // column auto increment
        count[selector] = ($(selector + content).find('[columnId]').length) + (count[selector] !== undefined ? 1 : 0);

        // assemble table column like :  [colItem | colItem | button]
        let column = ''
        for (let i = 0; i < columnSize - 1; i++) {
            // assemble table item
            if (i === 0) {
                column += firstColItem;
            } else {
                column += colItem;
            }
            column = column.replaceAll("{{name}}", selector + count[selector] + "-" + i)
                .replaceAll('{{buttonName}}', buttonNames[i - 1])
                .replaceAll("{{colname}}", colNames[i])

            // bind button upload event
            bindUploadEvent(selector + content,
                "[file-upload-button='" + selector + count[selector] + "-" + i + "']",
                "[file-input='" + selector + count[selector] + "-" + i + "']",
                "[selected-file='" + selector + count[selector] + "-" + i + "']");
        }
        // assemble button 
        column += buttonColItem;

        tabContent = tabContent
            .replace("{{colCount}}", count[selector])
            .replace("{{content}}", column)
        if ($(selector + "-btnGroup").length > 0) {
            $(selector + "-btnGroup").before(tabContent);
        } else
            $(selector + "-content").append(tabContent);
    }

    function removeRow(selector, type) {
        const element = $(selector)
        const row = element.parent().parent().parent();
        const rowId = row.attr('rowId');
        controllerComponent.deleteItem(rowId, type)
        row.remove()
    }

    function getTableData(selector) {
        const itemList = [];
        const rows = $(selector + content).find("[colcount]");
        for (let i = 0; i < rows.length; i++) {
            const item = {};
            const row = $(rows[i]);
            const cols = row.find("[name]");
            for (let j = 0; j < cols.length; j++) {
                const col = $(cols[j]);
                const colName = col.attr('colname')
                item[colName] = col.val();
            }
            item.id = row.attr('rowId')
            itemList.push(item)
        }
        return itemList;
    }

    function getAppKeyVaultData(selector) {
        const itemList = [];
        const rows = $(selector + content).find("[colcount]");
        for (let i = 0; i < rows.length; i++) {
            const item = {};
            const row = $(rows[i]);
            const cols = row.find("[name]");
            const col = $(cols[0]);
            item.id = row.attr('rowId');
            item.configKey = col.val();
            item.appId = Number($('#appId').attr('appId'));
            itemList.push(item);
        }
        return itemList;
    }

    function getDomainTableData(selector) {
        const itemList = [];
        const rows = $(selector + "-content").find(".m-data-table__item");
        for (let i = 0; i < rows.length; i++) {
            const item = {};
            const key = "#domain" + i;
            const domainName = $("[name='" + key + "-0']").val();
            const certification = $("[selected-file='" + key + "-1']").attr('data-filename');
            const privateKey = $("[selected-file='" + key + "-2']").attr('data-filename');
            const certificationFile = $("[file-input='" + key + "-1']")[0].files[0];
            const privateKeyFile = $("[file-input='" + key + "-2']")[0].files[0];

            item.domainName = domainName;
            item.certification = certification;
            item.privateKey = privateKey;

            item.certificationFile = certificationFile;
            item.privateKeyFile = privateKeyFile;
            itemList.push(item)
        }
        return itemList;
    }

    function getConfigMapFileData(selector) {
        const itemList = [];
        const rows = $(selector + "-content").find(".m-data-table__item");
        for (let i = 0; i < rows.length; i++) {
            const item = {};
            const key = "#configMapField" + i;
            const filePath = $("[name='" + key + "-0']").val();
            const fileLink = $("[selected-file='" + key + "-1']").attr('data-filename');
            item.filePath = filePath;
            item.fileLink = fileLink;
            item.id = $(rows[i]).attr('rowId')
            itemList.push(item)
        }
        return itemList;
    }

    function bindUploadEvent(selector, uploadButtonSelector, fileInputSelector, selectedFileNames) {

        // 解绑上传按钮的点击事件处理器
        $(selector).off('click', uploadButtonSelector);

        // 绑定上传按钮的点击事件处理器
        $(selector).on('click', uploadButtonSelector, function () {
            $(fileInputSelector).click(); // 触发文件输入的点击动作
        });

        // 解绑文件输入的变更事件处理器
        $(fileInputSelector).off('change');

        // 绑定文件输入的变更事件处理器
        $(fileInputSelector).on('change', function () {
            const selectedFiles = $(this).prop('files');
            if (selectedFiles.length > 0) {
                // Display the names of selected files
                const fileNames = [];
                for (let i = 0; i < selectedFiles.length; i++) {
                    fileNames.push(selectedFiles[i].name);
                }

                // Send the selected files to the server
                $(selectedFileNames).text(`${fileNames.join(', ')}`);
                controllerComponent.fileUpload(selectedFiles, selectedFileNames);
            } else {
                $(selectedFileNames).text('No files selected');
            }
        });
    }

    function validateTableContent(selector) {
        console.log('行の検証', selector);

        let flag = true;
        // 行の検証
        $(selector).each(function () {
            // 列の検証 input type
            $(this).find('input[type="text"]').each(function () {
                const col = $(this);
                const nextElement = col.next()
                if (!$(this).val()) {
                    if (nextElement.prop('tagName') !== "LABEL") {
                        col.css({"border-color": "red", "margin": "10px"})
                        col.after(`<label style="color:indianred">${i18next.t('appInfoPage.notNull1')}</label>`)
                    }
                    flag = false;
                } else {
                    if (col.next().prop('tagName') === 'LABEL') {
                        nextElement.remove()
                        col.css({"border-color": "", "margin-top": ""});
                    }
                }
            });
            // 列の検証 file upload
            $(this).find('input[type="file"]').each(function () {
                const col = $(this);
                const uploadButton = col.next().next()
                const space = col.next();
                // if (!$(this).val()) {
                if (!space.text()) {
                    if (uploadButton.next().prop('tagName') !== "LABEL") {
                        col.css({"border-color": "red", "margin": "10px"})
                        uploadButton.after(`<label style="color:indianred">${i18next.t('appInfoPage.notNull1')}</label>`)
                    }
                    flag = false;
                } else {
                    if (uploadButton.next().prop('tagName') === 'LABEL') {
                        uploadButton.next().remove()
                        col.css({"border-color": "", "margin-top": ""});
                    }
                }
            })
        })
        return flag;
    }

    function initDiskTable() {
        $('#diskConfig-add').off('click').on('click', function (e) {
            $("#diskConfig-content").append(`
                <div class="m-data-table__item" id="{{columnId}}" rowid="0" colcount="1">
                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                     <div id="diskType" class="a-pulldown">
                         <select name="diskType-selector" class="a-pulldown__select">
                             <option value="default"> Azure Standard SSD</option>
                             <option value="managed-csi"> Azure Standard SSD(CSI Driver)</option>
                             <option value="managed-premium"> Azure Premium SSD</option>
                             <option value="managed-csi-premium">Azure Premium SSD(CSI Driver)</option>
                         </select>
                         <div class="a-pulldown__icon-container">
                             <i class="a-icon a-icon--arrow-down"></i>
                         </div>
                     </div>
                 </span>
                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                     <div class="a-text-field a-text-field--type-text">
                         <input type="text" name="#diskConfig-1" value="" colname="path" class="a-text-field__input">
                     </div>
                 </span>
                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                     <div class="a-text-field a-text-field--type-text">
                         <input type="text" name="#diskConfig-2" value="" colname="size" class="a-text-field__input">
                     </div>
                 </span>
                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                     <span class="m-data-table__truncate-content">
                         <button class="a-button a-button--text">
                             <button type="button" class="a-add-item-button" onclick="tableComponent.removeRow(this, 'DiskInfo')">
                                 <i class="a-icon a-icon--close-hover"></i>
                             </button>
                     </span>
                 </span>
                </div>
        `)
        })
    }

    return {
        validateTableContent: validateTableContent,
        initComponent: init,
        initDiskTable: initDiskTable,
        bindUploadEvent: bindUploadEvent,
        getTableData: getTableData,
        getAppKeyVaultData: getAppKeyVaultData,
        getDomainTableData: getDomainTableData,
        getConfigMapFileData: getConfigMapFileData,
        removeRow: removeRow
    }
})(jQuery)