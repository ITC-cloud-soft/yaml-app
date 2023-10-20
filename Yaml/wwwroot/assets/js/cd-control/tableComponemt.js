const tableComponemt = (() => {
    
    function init(columnSize, selector) {
        
        $(selector+"-add").click(function () {
            addRow(columnSize, selector)
        })
    }

    function addRow(columnSize, selector) {
        
        let tabContent = 
            `
            <div class="m-data-table__item" id={{columnId}}>
                {{content}}
            </div>
            `
        let colItem = 
            `
             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                <div class="a-text-field a-text-field--type-text">    
                  <input type="text" name="{{name}}" value=""  class="a-text-field__input" />
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
            column += (colItem.replace("{{name}}", selector +  "-" + i))
        }
        // assemble button 
        column += buttonColItem;
        
        const columnId = $("").attr(selector + "-")
        
        tabContent = tabContent
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
        $(selector).find('')
        
    }

    return {
        initComponent: init,
        removeRow: removeRow
    }
})(jQuery)