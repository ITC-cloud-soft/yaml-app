function init (btn, spanElement, inputId){
   $(btn).off("click").on("click", function(){
       $(inputId).click()
   }) 
    
    $(inputId).on("click", function(){
        
    })
}