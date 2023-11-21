// 1. 编辑Cluster时 弹出的Cluster info 窗口显示寄存当前cluster的信息
//  按Cluster页面保存的按钮时 保存Cluster信息到appDataInfo

// 2.クラスターを新規追加する際に、クラスターページのデータをクリアする
// クラスターページの保存ボタンを押すと、クラスター情報をappDataInfoに保存する
$(function () {

    new Promise((resolve) => {
        resolve(initI18next());
    }).then(function () {
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
    $('#languageSwitcher').change(() => {
        const chosenLng = $(this).find("option:selected").attr('value');
        i18next.changeLanguage(chosenLng, () => {
            render();
            cdPlugin.bindEvents();
            cdPlugin.bindValidation(i18next);
            commonFunctions.userLanguage = chosenLng;
        });
    });

    // init table content
    tableComponemt.initComponent(2, selectors.keyVaultId, [], ["keyVault"], "AppKeyVault");
    tableComponemt.initComponent(2, selectors.clusterKeyVault, [], ["configKey"], "ClusterKeyVault");
    tableComponemt.initComponent(3, selectors.configMapId, [], ["configKey", "value"], "ConfigMap");
    tableComponemt.initComponent(3, selectors.diskConfig, [], ["name", "path"], "DiskInfo");
    tableComponemt.initComponent(4, selectors.domain, ["upload", "upload"], ["domainName", "certification", "privateKey", ""], "Domain");
    tableComponemt.initComponent(3, selectors.configMapField, ["upload"], ["filePath", "fileLink"], "ConfigFile");
    cdPlugin.getAppDataDtoFromBackend('1')
})

const cdPlugin = (($) => {
    "use strict"

    let clusterInfoList = [];
    let ifNewCluster = {
        flag: 1, // 1: new cluster 2: edit cached cluster 3: edit new cluster
        clusterId: 0,
    };

    let appFormValidator = {};
    let clusterFormValidator = {};

    function initElementEvent() {

        // bind new cluster btn event 
        $('#new-cluster-btn').off('click').click(() => {
            ifNewCluster.flag = 1;
            ifNewCluster.clusterId += -1;
            clearClusterPage();
            commonFunctions.showCustomModal("#modal-cluster")
            $('#clusterId').attr('clusterId', ifNewCluster.clusterId)
        })

        // bind confirm cluster event
        $("#confirmButton").off('click').click(() => {
            console.log(getClusterData())
            const clusterModalForm = $("#clusterForm");
            const keyVaultValid = tableComponemt.validateTableContent("#clusterKeyVault-content");
            const configMapValid = tableComponemt.validateTableContent("#configMap-content")
            const configFileValid = tableComponemt.validateTableContent("#configMapField-content")
            const domainValid = tableComponemt.validateTableContent("#domain-content")
            const diskInfoValid = tableComponemt.validateTableContent("#diskConfig-content")

            if (clusterModalForm.valid() && keyVaultValid && configMapValid && configFileValid && domainValid && diskInfoValid) {
                const clusterData = getClusterData();
                commonFunctions.closeCustomModal("#modal-cluster")
                clusterInfoList = clusterInfoList.filter(function (cluster) {
                    return cluster.id !== clusterData.id
                });
                clusterInfoList.push(clusterData);
                console.log(clusterData)
                console.log(getAppInfoData())
                renderAppPage(getAppInfoData())
            }
        })

        // bind cancel button event
        $("#cancelButton").off('click').click(() => {
            clearClusterPage();
            commonFunctions.closeCustomModal("#modal-cluster")
        })

        // bind save app info event
        $("#save-button").off('click').click(() => {
            const appForm = $("#appForm");
            if (appForm.valid() && tableComponemt.validateTableContent("#keyVault-content")) {
                const appInfoData = getAppInfoData()
                console.log({appInfoDto: appInfoData})
                commonFunctions.axios().post('/api/App/save',
                    {appInfoDto: appInfoData}
                ).then(function (res) {
                    console.log(res)
                    location.reload();
                }).catch(function (error) {
                    console.log(error)
                })
            }
        })

        // bind download json file event
        $('#download-button').off('click').click(function () {
            const appInfoData = getAppInfoData()
            commonFunctions.axios()
                .get(`/api/App/download?appId=${appInfoData.id}`, {responseType: 'blob'})
                .then(function (response) {
                    // Create a new Blob object using the response data
                    const fileBlob = new Blob([response.data], {type: 'application/json'});

                    // Create an object URL for the Blob
                    const objectUrl = URL.createObjectURL(fileBlob);

                    // Create a temporary anchor tag to trigger download
                    const tempLink = document.createElement('a');
                    tempLink.href = objectUrl;
                    tempLink.setAttribute('download', 'content.json'); // Set the file name for the download
                    document.body.appendChild(tempLink); // Append anchor to the body
                    tempLink.click(); // Simulate click on anchor to trigger download

                    // Clean up by revoking the Blob URL and removing the temporary anchor tag
                    URL.revokeObjectURL(objectUrl);
                    document.body.removeChild(tempLink);
                })
        })

        const fileInputSelector = '#jsonFileInput';
        $("#upload-button").off('click').click(function () {
            const fileInputSelector = '#jsonFileInput';
            $(fileInputSelector).click(); // Trigger file input click
        })

        $(fileInputSelector).off('onchange').change(function () {
            // files is a Dom property, use prop instead of attr
            const fileList = $(fileInputSelector).prop('files')
            if (fileList.length > 1) {
                alert('only one file is allowed')
                return
            }
            const file = fileList[0]
            const formData = new FormData();
            formData.append('files', file);

            // upload file
            commonFunctions.axios().post('/api/App/importJson', formData)
                .then(response => {
                    $(fileInputSelector).attr('files', '')
                    commonFunctions.showToast(3000, i18next.t('appInfoPage.jsonUploadSuccessText'), 'Green')
                }) 
                .catch(error => {
                    commonFunctions.showToast(3000, i18next.t('appInfoPage.jsonUploadErrorText'), 'indianred')
                    console.log(error);
                })
                .finally(() => {
                    $(fileInputSelector).val('');
                })
        })


        // bind help button event
        $('.a-help-button').off('click').click(function () {
            const helpText = $(this).attr('name');
            const parent = $(this).parent()
            let text = parent.find('[data-i18n]').text();
            text =  text ? text : parent.parent().text();
            const header = `<i class="a-icon a-icon--help"></i></br>${text} ${i18next.t('appInfoPage.is')}`
            commonFunctions.showModal(header, i18next.t(helpText))
        })
    }


    function renderClusterPage(clusterId) {
        clusterInfoList.forEach(function (cluster) {
            if (clusterId === cluster.id) {
                // clear cluster page if there is content
                clearClusterPage();
                commonFunctions.showCustomModal("#modal-cluster")
                $('#clusterId').attr('clusterId', clusterId);
                $(selectors.clusterName).val(cluster.clusterName)
                $(selectors.imageName).val(cluster.image)
                $(selectors.podCount).val(cluster.podNum)
                $('#cpu')[0].__component._choices.setChoiceByValue(cluster.cpu)
                $('#memory')[0].__component._choices.setChoiceByValue(cluster.memory)
                $(selectors.memory).val(cluster.memory)
                $(selectors.manageLevel).val(cluster.manageLabel)
                $(selectors.prefix).val(cluster.prefix)

                if (cluster.diskInfoFlag) {
                    $(selectors.diskCheckbox).prop('checked', true)
                    renderArea(cluster.diskInfoFlag, '#disk_div')
                    $('#diskSize')[0].__component._choices.setChoiceByValue(cluster.diskSize)
                    $('#diskType')[0].__component._choices.setChoiceByValue(cluster.diskClass)
                    for (let i = 0; i < cluster.diskInfoList.length; i++) {
                        $("#diskConfig-content").append(`
                            <div class="m-data-table__item" rowId="${cluster.diskInfoList[i].id}" id="{{columnId}}" colcount="${i + 1}">
                                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                    <div class="a-text-field a-text-field--type-text">    
                                      <input type="text"  name="#diskInfo-0" value="${cluster.diskInfoList[i].name}" colname="Name" class="a-text-field__input">
                                    </div>
                                </span>
                                 <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                    <div class="a-text-field a-text-field--type-text">    
                                      <input type="text" name="#diskInfo-1" value="${cluster.diskInfoList[i].path}" colname="Path" class="a-text-field__input">
                                    </div>
                                </span>
                                <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                    <span class="m-data-table__truncate-content">
                                        <button class="a-button a-button--text">
                                           </button><button type="button" class="a-add-item-button"><i class="a-icon a-icon--check-purple"></i></button> 
                                           <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this, 'DiskInfo')"><i class="a-icon a-icon--close-hover"></i></button> 
                                    </span>
                                </span>
                            </div>
                        `)
                    }
                }
                if (cluster.keyVaultFlag) {
                    $(selectors.KeyCheckbox).prop('checked', true)
                    renderArea(cluster.keyVaultFlag, '#KeyVault_div')
                    for (let i = 0; i < cluster.keyVault.length; i++) {
                        $('#clusterKeyVault-content').append(
                            `
                                <div class="m-data-table__item"  rowId="${cluster.keyVault[i].id}" id="{{columnId}}" colcount="${i + 1}">
                                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <div class="a-text-field a-text-field--type-text">    
                                          <input type="text" name="#clusterKeyVault-${i}" value="${cluster.keyVault[i].configKey}" colname="configKey" class="a-text-field__input">
                                        </div>
                                    </span>
                                    
                                    <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <span class="m-data-table__truncate-content">
                                            <button class="a-button a-button--text">
                                               </button><button type="button" class="a-add-item-button"><i class="a-icon a-icon--check-purple"></i></button> 
                                               <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this,'ClusterKeyVault')"><i class="a-icon a-icon--close-hover"></i></button> 
                                        </span>
                                    </span>
                                </div>
                            `
                        )
                    }
                }
                if (cluster.configMapFlag) {
                    $(selectors.configCheckbox).prop('checked', true)
                    renderArea(cluster.configMapFlag, '#Config_div')
                    for (let i = 0; i < cluster.configMap.length; i++) {
                        $('#configMap-content').append(
                            `
                                <div class="m-data-table__item" rowId="${cluster.configMap[i].id}" id="{{columnId}}" colcount="${i + 1}">
                                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <div class="a-text-field a-text-field--type-text">    
                                          <input type="text"  name="#configMap-0" value="${cluster.configMap[i].configKey}" colname="configKey" class="a-text-field__input">
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
                                               <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this, 'ConfigMap')"><i class="a-icon a-icon--close-hover"></i></button> 
                                        </span>
                                    </span>
                                </div>
                            `
                        )
                    }
                }
                if (cluster.configMapFileFlag) {
                    $(selectors.configMapFileCheckbox).prop('checked', true)
                    renderArea(cluster.configMapFileFlag, '#ConfigFile_div')
                    const configMapFiles = cluster.configFile;
                    for (let i = 0; i < configMapFiles.length; i++) {
                        let fileLink = configMapFiles[i].fileLink
                        if (fileLink.indexOf('_') > -1) {
                            fileLink = fileLink.split('_')[1]
                        }
                        // render page
                        $('#configMapField-content').append(
                            `
                                <div class="m-data-table__item" rowId="${configMapFiles[i].id}" columnid="{{columnId}}">
                                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <div class="a-text-field a-text-field--type-text">    
                                              <input type="text" name="#configMapField${i}-0" value="${configMapFiles[i].filePath}" colname="filePath" class="a-text-field__input">
                                        </div>
                                    </span>
                                    
                                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <div class="a-text-field a-text-field--type-text" style="max-width: 100px"> 
                                            <input type="file" file-input="#configMapField${i}-1" style="display: none"> 
                                            <span class="a-upload-field__description" data-filename="${configMapFiles[i].fileLink}" selected-file="#configMapField${i}-1"> ${fileLink} </span>
                                            <button type="button" class="a-button a-button--primary" file-upload-button="#configMapField${i}-1" colname="fileLink" name="#configMapField${i}-1">
                                                upload
                                            </button>
                                        </div>
                                    </span>
                                    
                                    <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                        <span class="m-data-table__truncate-content">
                                           <button type="button" class="a-button a-button--text" onclick="tableComponemt.removeRow(this, 'ConfigFile')">
                                             <div class="a-button__label"><i class="a-icon a-icon--close-hover"></i></div>
                                           </button> 
                                        </span>
                                    </span>
                                </div>
                            `
                        )

                        // bind upload event fn
                        tableComponemt.bindUploadEvent(
                            '#configMapField-content',
                            "[file-upload-button='#configMapField" + i + "-1']",
                            "[file-input='#configMapField" + i + "-1']",
                            "[selected-file='#configMapField" + i + "-1']",
                        )
                    }
                }
                if (cluster.domain != null) {
                    let certification = cluster.domain.certification
                    certification = certification.includes('_') ? certification.split('_')[1] : certification;

                    let privateKey = cluster.domain.privateKey
                    privateKey = privateKey.includes('_') ? privateKey.split('_')[1] : privateKey;

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
                                    <span class="a-upload-field__description" data-filename="${cluster.domain.certification}" selected-file="#domain0-1">${certification}</span>
                                    <button type="button" class="a-button a-button--primary" file-upload-button="#domain0-1" colname="certification" name="#domain0-1">
                                        upload
                                    </button>
                                </div>
                            </span>
                             <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                <div class="a-text-field a-text-field--type-text" style="max-width: 100px"> 
                                    <input type="file" file-input="#domain0-2" style="display: none"> 
                                    <span class="a-upload-field__description" data-filename="${cluster.domain.privateKey}" selected-file="#domain0-2">${privateKey}</span>
                                    <button type="button" class="a-button a-button--primary" file-upload-button="#domain0-2" colname="privateKey" name="#domain0-2">
                                        upload
                                    </button>
                                </div>
                            </span>
                            <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                                <span class="m-data-table__truncate-content">
                                   <button type="button" class="a-button a-button--text" onclick="tableComponemt.removeRow(this, 'Domain')">
                                     <div class="a-button__label"><i class="a-icon a-icon--close-hover"></i></div>
                                   </button> 
                                </span>
                            </span>
                        </div>
                    `)

                    // bind upload event fn
                    tableComponemt.bindUploadEvent(
                        '#domain-content',
                        "[file-upload-button='#domain" + 0 + "-1']",
                        "[file-input='#domain" + 0 + "-1']",
                        "[selected-file='#domain" + 0 + "-1']",
                    )
                    tableComponemt.bindUploadEvent(
                        '#domain-content',
                        "[file-upload-button='#domain" + 0 + "-2']",
                        "[file-input='#domain" + 0 + "-2']",
                        "[selected-file='#domain" + 0 + "-2']",
                    )
                }
                ifNewCluster.flag = 2
            }
        })
    }

    function renderArea(ifRender, selector) {
        ifRender ? $(selector).css({"display": "block"}) : $(selector).css({"display": "none"})
    }

    function getAppDataDtoFromBackend(appId) {
        commonFunctions.axios()
            .get(`/api/App/get?AppId=${appId}`)
            .then(function (response) {
                const appInfoDto = response.data;
                cdPlugin.renderPage(appInfoDto)
                console.log(appInfoDto)
                $('#appId').attr('appId', appInfoDto.id)
            }).catch(function (ex){
                console.error(ex)
        })
    }

    function renderAppPage(appInfoDto) {

        $("#keyVault-content").html('');
        $("#setting-table-content").html('');

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
                const keyVault = kvList[i];
                $('#keyVault-content').append(`
                 <div class="m-data-table__item" rowId="${keyVault.id}" id="{{columnId}}" colcount="1">
                     <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                        <div class="a-text-field a-text-field--type-text">    
                          <input type="text" name="#keyVault-${i}" value="${keyVault.configKey}" colname="keyVault" class="a-text-field__input">
                        </div>
                    </span>
                    
                    <span class="m-data-table__content m-data-table__content--type-data m-data-table__content--align-left m-data-table__content--valign-center">
                        <span class="m-data-table__truncate-content">
                            <button class="a-button a-button--text">
                               </button><button type="button" class="a-add-item-button"><i class="a-icon a-icon--check-purple"></i></button> 
                               <button type="button" class="a-add-item-button" onclick="tableComponemt.removeRow(this, 'AppKeyVault')"><i class="a-icon a-icon--close-hover"></i></button> 
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

    function initValidation(i18next) {
        /**
         * カスタム検証ルール
         */
        // app key vault checkbox validation
        validate(
            "appKeyVaultValidation",
            "#checkbox-single",
            "#keyVault-content"
        )

        // cluster disk info checkbox validation
        validate(
            "DiskInfoValidation",
            "#diskCheckbox",
            "#diskConfig-content"
        )

        // cluster page key vault checkbox validation
        validate(
            "clusterKeyVaultTableValidation",
            "#KeyCheckbox",
            "#clusterKeyVault-content"
        )

        // cluster config map vault checkbox validation
        validate(
            "ConfigMapValidation",
            "#ConfigCheckbox",
            "#configMap-content"
        )

        // cluster config file vault checkbox validation
        validate(
            "ConfigFileValidation",
            "#ConfigFileChk",
            "#configMapField-content"
        )

        // normal validation
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
            diskSize: {required: true},
            tenantId: {
                required: {
                    depends: function (element) {
                        return $(selectors.keyConnect).is(":checked");
                    }
                }
            },
            keyVault: {
                required: {
                    depends: function (element) {
                        return $(selectors.keyConnect).is(":checked");
                    }
                }
            },
            manageId: {
                required: {
                    depends: function (element) {
                        return $(selectors.keyConnect).is(":checked");
                    }
                }
            },
            keyConnect: {appKeyVaultValidation: true},
            KeyCheckbox: {clusterKeyVaultTableValidation: true},
            ConfigCheckbox: {ConfigMapValidation: true},
            ConfigMapFileCheckbox: {ConfigFileValidation: true},
            diskCheckbox: {DiskInfoValidation: true}
        }

        const messages = {
            appName: `${i18next.t('appInfoPage.appName')} ${i18next.t('appInfoPage.notNull')}`,
            crServer: `${i18next.t('appInfoPage.crServer')} ${i18next.t('appInfoPage.notNull')}`,
            token: `${i18next.t('appInfoPage.token')} ${i18next.t('appInfoPage.notNull')}`,
            mail: `${i18next.t('appInfoPage.mail')} ${i18next.t('appInfoPage.notNull')}`,
            clusterName: `${i18next.t('appInfoPage.clusterName')} ${i18next.t('appInfoPage.notNull')}`,
            imageName: `${i18next.t('appInfoPage.image')} ${i18next.t('appInfoPage.notNull')}`,
            podCount: `${i18next.t('appInfoPage.podNum')} ${i18next.t('appInfoPage.notNull')}`,
            cpu: ` cpu ${i18next.t('appInfoPage.notNull')}`,
            memory: `${i18next.t('appInfoPage.memory')} ${i18next.t('appInfoPage.notNull')}`,
            diskSize: `${i18next.t('appInfoPage.diskSize')} ${i18next.t('appInfoPage.notNull')}`,
            tenantId: `TenantId ${i18next.t('appInfoPage.notNull')}`,
            keyVault: `KeyVault ${i18next.t('appInfoPage.notNull')}`,
            manageId: `ManageId ${i18next.t('appInfoPage.notNull')}`,
            keyConnect: `${i18next.t('appInfoPage.tableContentNotNull')}`,
            KeyCheckbox: `${i18next.t('appInfoPage.tableContentNotNull')}`,
            ConfigCheckbox: `${i18next.t('appInfoPage.tableContentNotNull')}`,
            ConfigMapFileCheckbox: `${i18next.t('appInfoPage.tableContentNotNull')}`,
            diskCheckbox: `${i18next.t('appInfoPage.tableContentNotNull')}`,
            pwd: `${i18next.t('login-page.pwdEmpty')}`
        }

        const appForm = $("#appForm");
        appFormValidator = appForm.validate({
            focusCleanup: true,
            onkeyup: false,
            ignore: "",
            onfocusin: false,
            rules,
            messages,
            errorPlacement: function (error, element) {
                if (element.attr('id') === "checkbox-single") {
                    error.insertAfter($("#keyvalue_div"));
                } else {
                    error.insertAfter(element.parent());
                }
            }
        })

        const clusterModalForm = $("#clusterForm");
        clusterFormValidator = clusterModalForm.validate({
            focusCleanup: true,
            onkeyup: false,
            ignore: "",
            onfocusin: false,
            rules,
            messages,
            errorPlacement: function (error, element) {
                if (element.attr('id') === "KeyCheckbox"
                    || element.attr('id') === "keyConnect"
                    || element.attr('id') === "ConfigCheckbox"
                    || element.attr('id') === "ConfigFileChk"
                    || element.attr('id') === "diskCheckbox"
                ) {
                    error.insertAfter(element.parent());
                } else {
                    error.insertAfter(element.parent().parent().parent().parent());
                }
            }
        })

        clusterModalForm.validate().settings.messages = messages;
        appForm.validate().settings.messages = messages;
    }

    /**
     * カスタム検証ルール
     * @param metohdName
     * @param checkboxId
     * @param tableId
     * @param errorMessage
     */
    function validate(metohdName, checkboxId, tableId, errorMessage) {
        $.validator.addMethod(metohdName, function (value, element) {
            // true を返すと検証が成功したことを意味し、false を返すと検証に失敗したことを意味します。
            if ($(checkboxId).is(":checked")) {
                return $(tableId).find('.m-data-table__item').length > 0;
            }
            return true;
        }, errorMessage);
    }

    function renderErrorI18() {

    }

    function getAppInfoData() {
        const keyVaultAppData = tableComponemt.getAppKeyVaultData(selectors.keyVaultId)

        const appInfoDto = {};
        appInfoDto.id = Number($('#appId').attr('appId'));
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
        const clusterId = Number($('#clusterId').attr('clusterId'));
        const diskTableData = tableComponemt.getTableData(selectors.diskConfig)
        const keyVaultClusterData = tableComponemt.getTableData(selectors.clusterKeyVault)
        const configMapTableData = tableComponemt.getTableData(selectors.configMapId)
        const configMapFileTableData = tableComponemt.getConfigMapFileData(selectors.configMapField)
        const domainData = tableComponemt.getDomainTableData(selectors.domain)
        const diskInfoList = tableComponemt.getTableData(selectors.diskInfo)

        const clusterInfo = {};
        clusterInfo.id = clusterId;
        clusterInfo.clusterName = $(selectors.clusterName).val();
        clusterInfo.image = $(selectors.imageName).val();
        clusterInfo.podNum = Number($(selectors.podCount).val());
        clusterInfo.cpu = $(selectors.cpu).val();
        clusterInfo.memory = $(selectors.memory).val();
        clusterInfo.manageLabel = $(selectors.manageLevel).val();
        clusterInfo.prefix = $(selectors.prefix).val();
        clusterInfo.diskInfoFlag = $(selectors.diskCheckbox).prop("checked");
        clusterInfo.keyVaultFlag = $(selectors.KeyCheckbox).prop("checked");
        clusterInfo.configMapFlag = $(selectors.configCheckbox).prop("checked");
        clusterInfo.configMapFileFlag = $(selectors.configMapFileCheckbox).prop("checked");
        clusterInfo.configFile = configMapFileTableData;
        clusterInfo.configMap = configMapTableData;
        clusterInfo.keyVault = keyVaultClusterData;
        clusterInfo.diskInfoList = diskInfoList;
        if (domainData && domainData.length > 0) {
            clusterInfo.domain = domainData[0];
        } else {
            clusterInfo.domain = {}
        }

        clusterInfo.diskSize = $(selectors.diskSize).val();
        clusterInfo.diskClass = $(selectors.diskType).val();
        clusterInfo.diskInfoList = diskTableData;
        return clusterInfo;
    }

    function fileUpload(fileList, selector) {
        const selectedFiles = $(this).prop('files');
        const formData = new FormData();
        for (let i = 0; i < fileList.length; i++) {
            formData.append('files', fileList[i]);
        }
        commonFunctions.axios().post('/api/App/uploadFiles', formData)
            .then(response => {
                $(selector).attr("data-filename", response.data.files[0])
            })
            .catch(error => {
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
        $(selectors.configCheckbox).prop('checked', false);
        $(selectors.configMapFileCheckbox).prop('checked', false);
        $(selectors.diskSize).val('')
        $(selectors.diskCheckbox).prop('checked', false)

        // clear table content and unbind button's event
        const configMapFieldContent = $('#configMapField-content').find('.m-data-table__container-item')[0];
        $(configMapFieldContent).html('')
        $('#domain-content').html('')
        $('#configMap-content').html('')
        $('#clusterKeyVault-content').html('')
        $('#diskConfig-content').html('')

        // resetForm
        appFormValidator.resetForm();
        clusterFormValidator.resetForm();
    }

    function deleteItem(id, type) {
        if (typeof id === "undefined") {
            console.log("id not exist")
            return
        }
        commonFunctions.axios().delete(`/api/App/deleteItem?id=${id}&type=${type}`)
            .then(function (response) {
                // TODO delete success bubble
                console.log(response)
            });
    }

    return {
        deleteItem: deleteItem,
        bindValidation: initValidation,
        getAppDataDtoFromBackend: getAppDataDtoFromBackend,
        renderPage: renderAppPage,
        fileUpload: fileUpload,
        renderClusterPage: renderClusterPage,
        bindEvents: initElementEvent,
    };
})(jQuery)

