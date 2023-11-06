// 1. 编辑Cluster时 弹出的Cluster info 窗口显示寄存当前cluster的信息
//  按Cluster页面保存的按钮时 保存Cluster信息到appDataInfo
// 2. 新增cluster时 清空cluster 页面数据
//  按Cluster页面保存的按钮时 保存Cluster信息到appDataInfo
$(function () {
    new Promise((resolve, reject) => {
        resolve(initI18next());
    }).then(function (result) {
        cdPlugin.bindEvents();
        cdPlugin.bindValidation(i18next);
    })

    // 阻止表单的默认提交行为
    $('#appForm').submit(function (event) {
        event.preventDefault();
    });
    $('#clusterForm').submit(function (event) {
        event.preventDefault();
    });

    // bind switch language event
    $('#languageSwitcher').change((a, b, c) => {
        const chosenLng = $(this).find("option:selected").attr('value');
        i18next.changeLanguage(chosenLng, () => {
            render();
            cdPlugin.renderErrorI18();
            commonFunctions.userLanguage = chosenLng;
        });
    });

    // init table content
    tableComponemt.initComponent(2, selectors.keyVaultId, [], ["keyVault"]);
    tableComponemt.initComponent(2, selectors.clusterKeyVault, [], ["configKey"]);
    tableComponemt.initComponent(3, selectors.configMapId, [], ["configKey", "value"]);
    tableComponemt.initComponent(3, selectors.diskConfig, [], ["name", "path"]);
    tableComponemt.initComponent(4, selectors.domain, ["upload", "upload"], ["domainName", "certification", "privateKey", ""]);
    tableComponemt.initComponent(3, selectors.configMapField, ["upload"], ["filePath", "fileLink"]);
    cdPlugin.getAppDataDtoFromBackend('1')
})


const cdPlugin = (($) => {
    "use strict"

    let clusterInfoList = [];

    function renderClusterPage(clusterId) {
        clusterInfoList.forEach(function (cluster) {
            if (clusterId === cluster.id) {
                commonFunctions.showCustomModal("#modal-cluster")
                $(selectors.clusterName).val(cluster.clusterName)
                $(selectors.imageName).val(cluster.image)
                $(selectors.podCount).val(cluster.podNum)
                $(selectors.cpu).val(cluster.cpu)
                $(selectors.memory).val(cluster.memory)
                $(selectors.manageLevel).val(cluster.manageLabel)
                $(selectors.prefix).val(cluster.prefix)

                if (cluster.diskInfoFlag) {
                    $(selectors.diskCheckbox).prop('checked', true)
                }
                if (cluster.keyVaultFlag) {
                    $(selectors.KeyCheckbox).prop('checked', true)
                    renderArea(cluster.keyVaultFlag, '#KeyVault_div')
                    for (let i = 0; i < cluster.keyVault.length; i++) {
                        console.log(cluster.keyVault.length)
                        $('#clusterKeyVault-content').append(
                            `
                                <div class="m-data-table__item" id="{{columnId}}" colcount="${i + 1}">
                                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <div class="a-text-field a-text-field--type-text">    
                                          <input type="text" name="#clusterKeyVault-${i}" value="${cluster.keyVault[i].configKey}" colname="configKey" class="a-text-field__input">
                                        </div>
                                    </span>
                                    
                                    <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <span class="m-data-table__truncate-content">
                                            <button class="a-button a-button--text">
                                               </button><button type="button" class="a-add-item-button"><i class="a-icon a-icon--check-purple"></i></button> 
                                               <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this)"><i class="a-icon a-icon--close-hover"></i></button> 
                                        </span>
                                    </span>
                                </div>
                            `
                        )
                    }
                }
                if (cluster.configMapFlag) {
                    $(selectors.configCheckbox).prop('checked', true)
                    renderArea(cluster.keyVaultFlag, '#Config_div')
                    for (let i = 0; i < cluster.configMap.length; i++) {
                        $('#configMap-content').append(
                            `
                                <div class="m-data-table__item" id="{{columnId}}" colcount="${i + 1}">
                                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <div class="a-text-field a-text-field--type-text">    
                                          <input type="text" name="#configMap-0" value="${cluster.configMap[i].configKey}" colname="configKey" class="a-text-field__input">
                                        </div>
                                    </span>
                                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <div class="a-text-field a-text-field--type-text">    
                                          <input type="text" name="#configMap-1" value="${cluster.configMap[i].value}" colname="value" class="a-text-field__input">
                                        </div>
                                    </span>
                                    <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <span class="m-data-table__truncate-content">
                                            <button class="a-button a-button--text">
                                               </button><button type="button" class="a-add-item-button"><i class="a-icon a-icon--check-purple"></i></button> 
                                               <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this)"><i class="a-icon a-icon--close-hover"></i></button> 
                                        </span>
                                    </span>
                                </div>
                            `
                        )
                    }
                }
                if (cluster.configMapFileFlag) {
                    $(selectors.configMapFileCheckbox).prop('checked', true)
                    renderArea(cluster.keyVaultFlag, '#ConfigFile_div')
                    const configMapFiles = cluster.configFile;
                    for (let i = 0; i < configMapFiles.length; i++) {
                        // render page
                        $('#configMapField-table-body').append(`
                            <div class="m-data-table__item" columnid="{{columnId}}">
                                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                    <div class="a-text-field a-text-field--type-text">    
                                          <input type="text" name="#configMapField${i}-0" value="${configMapFiles[i].filePath}" colname="filePath" class="a-text-field__input">
                                    </div>
                                </span>
                                
                                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                    <div class="a-text-field a-text-field--type-text" style="max-width: 100px"> 
                                        <input type="file" file-input="#configMapField${i}-1" style="display: none"> 
                                        <span class="a-upload-field__description" selected-file="#configMapField${i}-1"> ${configMapFiles[i].fileLink} </span>
                                        <button type="button" class="a-button a-button--primary" file-upload-button="#configMapField${i}-1" colname="fileLink" name="#configMapField${i}-1">
                                            upload
                                        </button>
                                    </div>
                                </span>
                                
                                <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                    <span class="m-data-table__truncate-content">
                                       <button type="button" class="a-button a-button--text" onclick="tableComponemt.removeRow(this)">
                                         <div class="a-button__label"><i class="a-icon a-icon--close-hover"></i></div>
                                       </button> 
                                    </span>
                                </span>
                            </div>
                        `)

                        // bind upload event fn
                        tableComponemt.bindUploadEvent(
                            '#configMapField-content',
                            "[file-upload-button='#configMapField" + i + "-1']",
                            "[file-input='#configMapField" + i + "-1']",
                            "[selected-file='#configMapField" + i + "-1']",
                        )
                    }
                }
                
                if (cluster.domain != null){
                    $('#domain-content').append(`
                        <div class="m-data-table__item" columnid="{{columnId}}">
                             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                <div class="a-text-field a-text-field--type-text">    
                                      <input type="text" name="#domain0-0" value="${cluster.domain.domainName}" colname="domainName" class="a-text-field__input">
                                </div>
                            </span>
                             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                <div class="a-text-field a-text-field--type-text" style="max-width: 100px"> 
                                    <input type="file" file-input="#domain0-1" style="display: none"> 
                                    <span class="a-upload-field__description" selected-file="#domain0-1">${cluster.domain.certification}</span>
                                    <button type="button" class="a-button a-button--primary" file-upload-button="#domain0-1" colname="certification" name="#domain0-1">
                                        upload
                                    </button>
                                </div>
                            </span>
                             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                <div class="a-text-field a-text-field--type-text" style="max-width: 100px"> 
                                    <input type="file" file-input="#domain0-2" style="display: none"> 
                                    <span class="a-upload-field__description" selected-file="#domain0-2">${cluster.domain.privateKey}</span>
                                    <button type="button" class="a-button a-button--primary" file-upload-button="#domain0-2" colname="privateKey" name="#domain0-2">
                                        upload
                                    </button>
                                </div>
                            </span>
                            <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                <span class="m-data-table__truncate-content">
                                   <button type="button" class="a-button a-button--text" onclick="tableComponemt.removeRow(this)">
                                     <div class="a-button__label"><i class="a-icon a-icon--close-hover"></i></div>
                                   </button> 
                                </span>
                            </span>
                        </div>
                    
                    `)

                    // bind upload event fn
                    tableComponemt.bindUploadEvent(
                        '#configMapField-content',
                        "[file-upload-button='#configMapField" + 0 + "-1']",
                        "[file-input='#configMapField" + 0 + "-1']",
                        "[selected-file='#configMapField" + 0 + "-1']",
                    )
                }
            }
        })
    }

    function renderArea(ifRender, selector) {
        ifRender ? $(selector).css({"display": "block"}) : $(selector).css({"display": "none"})
    }

    function getAppDataDtoFromBackend(appId) {

        commonFunctions.axios()
            .get(`/api/App/${appId}`)
            .then(function (response) {
                console.log(response)
                console.log(response.data)
                cdPlugin.renderPage(response.data)
            })
    }

    function renderPage(appInfoDto) {

        clusterInfoList = appInfoDto.clusterInfoList;

        // App Info Page
        $(selectors.appName).val(appInfoDto.appName)
        $(selectors.crServer).val(appInfoDto.cr)
        $(selectors.token).val(appInfoDto.token)
        $(selectors.mail).val(appInfoDto.mailAddress)
        if (appInfoDto.keyVaultFlag) {
            $(selectors.keyConnect).prop("checked", "checked");
            $(selectors.tenantId).val(appInfoDto.keyVault.tenantId)
            $(selectors.keyVault).val(appInfoDto.keyVault.keyVaultName)
            $(selectors.manageId).val(appInfoDto.keyVault.managedId)
            renderArea(appInfoDto.keyVaultFlag, '#keyvalueDiv')
            renderArea(appInfoDto.keyVaultFlag, '#keyvalue_div')
            const kvList = appInfoDto.keyVault.keyVault;
            for (let i = 0; i < kvList.length; i++) {
                $('#keyVault-content').append(`
                 <div class="m-data-table__item" id="{{columnId}}" colcount="1">
                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                        <div class="a-text-field a-text-field--type-text">    
                          <input type="text" name="#keyVault-${i}" value="${kvList[i]}" colname="keyVault" class="a-text-field__input">
                        </div>
                    </span>
                    
                    <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                        <span class="m-data-table__truncate-content">
                            <button class="a-button a-button--text">
                               </button><button type="button" class="a-add-item-button"><i class="a-icon a-icon--check-purple"></i></button> 
                               <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this)"><i class="a-icon a-icon--close-hover"></i></button> 
                        </span>
                    </span>
                </div>
            `)
            }
        }
        if (appInfoDto.netdataFlag) {
            $(selectors.Netdata).attr("checked", "checked");
        }
        // render cd-controller cluster info page
        if (appInfoDto.clusterInfoList != null) {
            appInfoDto.clusterInfoList.forEach(function (cluster) {
                $('#setting-table-content').append(`
                    <div class="m-data-table__item">
                        <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                          <span class="m-data-table__truncate-content">${cluster.clusterName}</span>
                        </span>
                        <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                          <span class="m-data-table__truncate-content">${cluster.image}</span>
                        </span>
                        <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                          <span class="m-data-table__truncate-content">${cluster.podNum}</span>
                        </span>
                        <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                          <span class="m-data-table__truncate-content">${cluster.cpu}</span>
                        </span>
                        <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                          <span class="m-data-table__truncate-content">${cluster.memory}</span>
                        </span>
                        <span class="m-data-table__content m-data-table__content--type-action m-data-table__content--align-left m-data-table__content--valign-center">
                          <span class="m-data-table__truncate-content">
                              <button class="a-button a-button--text" onclick="cdPlugin.renderClusterPage(${cluster.id})">
                                  <div class="a-button__label">編集</div>
                              </button>
                              <button class="a-button a-button--text" onclick="cdPlugin.renderClusterPage(${cluster.id})">
                                  <div class="a-button__label">削除 </div>
                              </button>
                          </span>
                        </span>
                    </div>
                `)
            })
        }
        
        
    }

    function initElementEvent() {

        // new cluster show modal event
        // 新しい クラスター が モーダル イベント
        $('#new-cluster-btn').click(() => {
            clearClusterPage();
            commonFunctions.showCustomModal("#modal-cluster")
        })

        // save
        $("#confirmButton").click(() => {
            const clusterModalForm = $("#clusterForm");
            // if(clusterModalForm.valid()){
            //     const clusterData = getClusterData();
            //     commonFunctions.closeCustomModal("#modal-cluster")
            // }
            const clusterData = getClusterData();
            console.log(clusterData);
            clusterInfoList.push(clusterData);

            const appInfoData = getAppInfoData()
            console.log({appInfoDto: appInfoData})

            const files = [];
            const file = $("[file-input='#domain0-1']")[0].files[0];
            files.push(appInfoData.clusterInfoList[0].domain.certificationFile)
            files.push(appInfoData.clusterInfoList[0].domain.privateKeyFile)
            fileUpload(files)

            commonFunctions.axios().post('/api/App/save',
                {appInfoDto: appInfoData}
            ).then(function amlApp(res) {
                console.log(res)
            }).catch(function (error) {
                console.log(error)
            })
            // clearClusterPage();
        })

        $("#cancelButton").click(() => {
            commonFunctions.closeCustomModal("#modal-cluster")
        })

        $("#save-button").click(() => {
            const appInfoData = getAppInfoData()
            console.log({appInfoDto: appInfoData})
            commonFunctions.axios().post('/api/App/save',
                {appInfoDto: appInfoData}
            ).then(function amlApp(res) {
                console.log(res)
            }).catch(function (error) {
                console.log(error)
            })
        })
    }

    function initValidation(i18next) {

        const rules = {
            appName: {required: true},
            crServer: {required: true},
            token: {required: true},
            mail: {required: true},
            clusterName: {required: true},
            imageName: {required: true},
            podCount: {required: true},
            cpu: {required: true},
            memory: {required: true},
        }

        const fieldName = i18next.t('appInfoPage.appName');
        const appName = i18next.t('common.notNull', {fieldName});
        const messages = {
            appName: appName,
            pwd: {
                required: i18next.t('login-page.pwdEmpty')
            }
        }

        const appForm = $("#appForm");
        appForm.validate({
            focusCleanup: true,
            onkeyup: false,
            ignore: "",
            onfocusin: false,
            rules,
            messages,
            errorPlacement: function (error, element) {
                error.insertAfter(element.parent());
            }
        })

        const clusterModalForm = $("#clusterForm");
        clusterModalForm.validate({
            focusCleanup: true,
            onkeyup: false,
            ignore: "",
            onfocusin: false,
            rules,
            messages,
            errorPlacement: function (error, element) {
                error.insertAfter(element.parent());
            }
        })
    }

    function renderErrorI18() {

    }

    function getAppInfoData() {
        const keyVaultAppData = tableComponemt.getAppKeyVaultData(selectors.keyVaultId)

        const appInfoDto = {};
        appInfoDto.appName = $(selectors.appName).val();
        appInfoDto.cr = $(selectors.crServer).val();
        appInfoDto.token = $(selectors.token).val();
        appInfoDto.mailAddress = $(selectors.mail).val();
        appInfoDto.keyVaultFlag = $(selectors.keyConnect).prop("checked");
        appInfoDto.keyVault = {};
        appInfoDto.keyVault.tenantId = $(selectors.tenantId).val();
        appInfoDto.keyVault.managedId = $(selectors.manageId).val();
        appInfoDto.keyVault.keyVault = keyVaultAppData;
        appInfoDto.keyVault.keyVaultName = $(selectors.keyVault).val();
        appInfoDto.netdataFlag = $(selectors.Netdata).prop("checked");
        appInfoDto.clusterInfoList = clusterInfoList;
        return appInfoDto;
    }

    function getClusterData() {
        const diskTableData = tableComponemt.getTableData(selectors.diskConfig)
        const keyVaultClusterData = tableComponemt.getTableData(selectors.clusterKeyVault)
        const configMapTableData = tableComponemt.getTableData(selectors.configMapId)
        const configMapFileTableData = tableComponemt.getConfigMapFileData(selectors.configMapField)
        const domainData = tableComponemt.getDomainTableData(selectors.domain)

        console.log(diskTableData);
        console.log(keyVaultClusterData);
        console.log(configMapTableData);
        console.log(domainData);
        console.log(configMapFileTableData);

        const clusterInfo = {};
        clusterInfo.clusterName = $(selectors.clusterName).val();
        clusterInfo.image = $(selectors.imageName).val();
        clusterInfo.podNum = Number($(selectors.podCount).val());
        clusterInfo.cpu = $(selectors.cpu).val();
        clusterInfo.memory = $(selectors.memory).val();
        clusterInfo.manageLabel = $(selectors.manageLevel).val();
        clusterInfo.prefix = $(selectors.prefix).val();
        clusterInfo.diskInfoFlag = $(selectors.diskCheckbox).prop("checked");
        clusterInfo.keyVaultFlag = $(selectors.keyVaultClusterPageCheckbox).prop("checked");
        clusterInfo.configMapFlag = $(selectors.configCheckbox).prop("checked");
        clusterInfo.configMapFileFlag = $(selectors.configMapFileCheckbox).prop("checked");
        clusterInfo.configFile = configMapFileTableData;
        clusterInfo.configMap = configMapTableData;
        clusterInfo.keyVault = keyVaultClusterData;
        if (domainData && domainData.length > 0) {
            clusterInfo.domain = domainData[0];
        } else {
            clusterInfo.domain = {}
        }

        clusterInfo.disk = {};
        clusterInfo.disk.size = $(selectors.diskSize).val();
        clusterInfo.disk.type = $(selectors.diskType).val();
        clusterInfo.disk.mountPath = diskTableData;
        return clusterInfo;
    }

    function fileUpload(fileList) {
        const formData = new FormData();
        for (let i = 0; i < fileList.length; i++) {
            formData.append('files', fileList[i]);

        }
        // 发送POST请求
        axios.post('/api/App/uploadFiles', formData, {
            // headers: {
            //     'Content-Type': 'multipart/form-data', // 设置Content-Type为multipart/form-data
            // },
        })
            .then(response => {
                // 请求成功的处理逻辑
                console.log(response.data);
            })
            .catch(error => {
                // 请求失败的处理逻辑
                console.log(error);
            });
    }

    function clearClusterPage() {
        $(selectors.clusterName).val('');
        $(selectors.imageName).val('');
        $(selectors.podCount).val('');
        $(selectors.cpu).val('');
        $(selectors.memory).val('');
        $(selectors.manageLevel).val('');
        $(selectors.prefix).val('');
        $(selectors.diskCheckbox).val('');
        $(selectors.keyVaultClusterPageCheckbox).val('');
        $(selectors.configCheckbox).val('');
        $(selectors.configMapFileCheckbox).val('')
        // remove table data
        // $(selectors.domainContent).find().forEach()
        const configMapFieldContent = $('#configMapField-content')
            .find('.m-data-table__container-item')[0];
        $(configMapFieldContent).html('')
    }

    function getDomainTableData() {
        $('#configMapField-content').find()
    }

    return {
        bindValidation: initValidation,
        getAppDataDtoFromBackend: getAppDataDtoFromBackend,
        renderPage: renderPage,
        renderClusterPage: renderClusterPage,
        renderErrorI18: renderErrorI18,
        bindEvents: initElementEvent,
    };
})(jQuery)

