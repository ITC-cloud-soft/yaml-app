const tableComponemt = (() => {
    const content = '-content';
    const count = {
        
    };
    
    function init(columnSize, selector, buttonNames, colNames) {
        $(selector+"-add").click(function () {
            if(buttonNames && buttonNames.length > 0){
                addRowWithButton(columnSize, selector, buttonNames, colNames)
            }else{
                addRow(columnSize, selector, colNames)
            }
        })
    }

    function addRow(columnSize, selector, colNames) {
        
        let tabContent = 
            `
            <div class="m-data-table__item" id={{columnId}} colCount={{colCount}}>
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
                       <button type="button" class="a-add-item-button"><i class="a-icon a-icon--check-purple"></i></button> 
                       <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this)"><i class="a-icon a-icon--close-hover"></i></button> 
                    </button>
                </span>
            </span>
            `
        
        // assemble table column like :  [colItem | colItem | button]
        let column = ''
        for (let i = 0; i < columnSize - 1; i++) {
            // assemble table item
            column += (colItem
                .replace("{{name}}", selector +  "-" + i)
                .replace("{{colname}}", colNames[i])
            )
        }
        column += buttonColItem;
        
        // column auto increment
        if(!count[selector]){
            count[selector] = 0;
        }else {
            count[selector] = $(selector + content).find(`[colcount]`).length;
        }
        count[selector]++;
       
        tabContent = tabContent
            .replace("{{colCount}}", count[selector] )
            .replace("{{content}}", column)
        if( $(selector + "-btnGroup").length > 0) {
            $(selector + "-btnGroup").before(tabContent);
        }else 
          $(selector+"-content").append(tabContent);
    }
    
    function addRowWithButton(columnSize, selector, buttonNames, colNames){

        let tabContent =
            `
            <div class="m-data-table__item" columnId={{columnId}}>
                {{content}}
            </div>
            `
        
        let colItem =
            `
             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                <div class="a-text-field a-text-field--type-text" style="max-width: 100px"> 
                    <input type="file" file-input="{{name}}"  style="display: none" /> 
                    <span class="a-upload-field__description" selected-file="{{name}}"></span>
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
                   <button type="button" class="a-button a-button--text" onclick="tableComponemt.removeRow(this)">
                     <div class="a-button__label"><i class="a-icon a-icon--close-hover"></i></div>
                   </button> 
                </span>
            </span>
            `     
        // column auto increment
        if(count[selector] === undefined){
            count[selector] = 0;
        }else {
            count[selector] = $(selector + content).find('[columnId]').length + 1;
        }

        // assemble table column like :  [colItem | colItem | button]
        let column = ''
        for (let i = 0; i < columnSize - 1; i++) {
            // assemble table item
            if(i === 0){
                column += firstColItem;
            }else{
                column += colItem;
            }
            column = column.replaceAll("{{name}}", selector + count[selector] +  "-" + i)
                .replaceAll('{{buttonName}}', buttonNames[i-1])
                .replaceAll("{{colname}}", colNames[i])
            
            // bind button upload event
            bindUploadEvent(selector + content, 
                "[file-upload-button='" + selector + count[selector] +  "-" + i + "']",
                "[file-input='" +  selector + count[selector] +  "-" + i + "']",
                "[selected-file='" +  selector + count[selector] +  "-" + i + "']");
        }
        // assemble button 
        column += buttonColItem;

        tabContent = tabContent
            .replace("{{colCount}}", count[selector] )
            .replace("{{content}}", column)
        if( $(selector + "-btnGroup").length > 0) {
            $(selector + "-btnGroup").before(tabContent);
        }else
            $(selector+"-content").append(tabContent);
    }
    
    function removeRow(selector){
        const element = $(selector)
        element.parent().parent().parent().remove()
    }
    
    function getTableData(selector){
        const itemList = [];
        const rows = $(selector + content).find("[colcount]");
        for (let i = 0; i < rows.length; i++) {
            const item = {};
            const row = $(rows[i]);
            const cols =row.find("[name]");
            for (let j = 0; j < cols.length; j++) {
                const col = $(cols[j]);
                const colName = col.attr('colname')
                item[colName] = col.val();
            }
            console.log(item)
            itemList.push(item)
        }
        return itemList;
    }

    function getTableDataWithFileUpload(selector){
        const itemList = [];
        const rows = $(selector + content).find(".m-data-table__item");
        for (let i = 0; i < rows.length; i++) {
            const item = {};
            const row = $(rows[i]);
            row.find()
            console.log(item)
            itemList.push(item)
        }
        return itemList;
    }
    
    function bindUploadEvent(selector, uploadButtonSelector, fileInputSelector, selectedFileNames){
        console.log(uploadButtonSelector)
        console.log(fileInputSelector)
        console.log(selectedFileNames)
        
        $(selector).on('click', uploadButtonSelector, function() {
            console.log(" Trigger file input click")
            $(fileInputSelector).click(); // Trigger file input click
        });

        // Handle file selection and upload
        $(selector).on('change', fileInputSelector, function() {
            const selectedFiles = $(this).prop('files');
            if (selectedFiles.length > 0) {
                // Display the names of selected files
                const fileNames = [];
                for (let i = 0; i < selectedFiles.length; i++) {
                    fileNames.push(selectedFiles[i].name);
                }
                $(selectedFileNames).text(`  ${fileNames.join(', ')}`);
                // Send the selected files to the server
                // uploadFiles(selectedFiles);
            } else {
                $(selectedFileNames).text('No files selected');
            }
        });
    }
    
    return {
        initComponent: init,
        getTableData: getTableData,
        getTableDataWithFileUpload: getTableDataWithFileUpload,
        removeRow: removeRow
    }
})(jQuery)