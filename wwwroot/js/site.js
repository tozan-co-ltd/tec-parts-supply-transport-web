// url取得
var baseUrl = window.location.origin;
var pathName = window.location.pathname.split('/');
if (pathName.length > 2)
    baseUrl = baseUrl + "/" + pathName[1];

// -----------------------------------準備画面-----------------------------------//
// SignalRを使用して接続を初期化する
const isPreparationPage = document.getElementById("parts-page");
if (isPreparationPage) {
    var connectionSupply = new signalR.HubConnectionBuilder().withUrl("partsHub").build();
    $(function () {
        connectionSupply.start().then(function () {
            InvokeSupplys();
        })
    });

    // 短い遅延後に再接続を試みる
    var closeConnectSupplyCount = 0;
    connectionSupply.onclose(function (error) {
        setTimeout(function () {
            connectionSupply.start().then(function () {
                InvokeSupplys();
            })
            closeConnectSupplyCount += 1;
            console.log("Error - onclose 再接続" + closeConnectSupplyCount + "回目");
            // 接続が2回以上失われた場合はページをリロード
            if (closeConnectSupplyCount >= 2)
                window.location.reload();
        }, 500);
    });

    // ページを離れた時やリロードしたタイミングで接続を止める
    window.addEventListener('unload', function () {
        console.log("addEventListener - unload");
        connectionSupply.stop();
    });

    // ハブのメソッドを呼び出す
    function InvokeSupplys() {
        connectionSupply.invoke("SendParts").catch(function (error) {
            // Controllerに接続できない場合はエラー
            console.log("Error - invoke catch");
            $(".connectionSupplyError").text(error);
            $(".connectionSupplyError").show();
        });
    }

    // エラー発生時
    connectionSupply.on("Error", (error) => {
        console.log("Error - on");
        $(".connectionSupplyError").text(error);
        $(".connectionSupplyError").show();
    });


    // グリッドに依頼をバインドする
    connectionSupply.on("ReceivedSupplys", function (supplys) {
        BindSupplysToGrid(supplys);
    });
}
// グリッドに依頼をバインドする
function BindSupplysToGrid(supplys) {
    $('#tblSupplyLeft tbody').empty();
    $('#tblSupplyRight tbody').empty();

    var tableLeftDom = document.getElementById('tblSupplyLeft');
    var tableRightDom = document.getElementById('tblSupplyRight');

    if (tableLeftDom !== null) {
        var table = tableLeftDom.getElementsByTagName('tbody')[0];
        var table1 = tableRightDom.getElementsByTagName('tbody')[0];

        const renderedTotalButtons = new Set();
        const completedMachines = new Set();

        // 左のテーブル取得
        var supplys1 = supplys.slice(0, 6);
        createTable(supplys1, table);

        // 右のテーブル取得
        var supplysTmp = supplys.length - supplys1.length;
        if (supplysTmp > 0) {
            var supplys2 = supplys.slice(6, 12);
            createTable(supplys2, table1);
        }

        // テーブルを作成
        function createTable(supplys, table) {
            if (supplys.length > 0) {
                $(".supplyContent").show();
                $(".noneDateMess").hide();

                for (let i = 0; i < supplys.length; i++) {
                    var row = table.insertRow();
                    var cell1 = row.insertCell(0);
                    var cell2 = row.insertCell(1);
                    var cell3 = row.insertCell(2);
                    var cell4 = row.insertCell(3);
                    var cell5 = row.insertCell(4);
                    var cell6 = row.insertCell(5);
                    var cell7 = row.insertCell(6);
                    var cell8 = row.insertCell(7);
                    var cell9 = row.insertCell(8);

                    // 現在日時
                    var today = new Date();
                    // 依頼時間
                    var resDateTime = new Date(supplys[i].correctedRequestDatetime);
                    var elapsedMs = today - resDateTime; // 過ぎました時間
                    var countdownMs = supplys[i].countDownTime * 60000; // カウント時間
                    var remainingMs = countdownMs - elapsedMs; // 残り時間
                    var dataTime = elapsedMs; // 経過時間
                    var subResult;

                    // 時間表示
                    if (elapsedMs >= 60 * 60000) { // 1時間経過した場合、表示を「59:59」で固定
                        subResult = "59:59";
                    } else if (remainingMs > 0) {　// カウントダウン中（残り時間がまだある）
                        const minutes = Math.floor(remainingMs / 60000);
                        const seconds = Math.floor((remainingMs % 60000) / 1000);
                        subResult = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
                    } else {　// カウントアップ中（カウントダウン終了後の経過時間を表示）
                        const totalMinutes = supplys[i].countDownTime + Math.floor((elapsedMs - countdownMs) / 60000);
                        const seconds = Math.floor((elapsedMs % 60000) / 1000);
                        const displayMinutes = Math.min(totalMinutes, 59);
                        subResult = `${displayMinutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
                    }

                    cell2.innerHTML = `${supplys[i].machineNum}`;
                    cell2.className = 'machine-number';

                    //cell3.innerHTML = `${supplys[i].boxType}`;
                    cell3.innerHTML = `${supplys[i].address}`;
                    cell3.className = 'address';

                    //cell4.innerHTML = `${supplys[i].boxType}`;
                    cell4.innerHTML = `${supplys[i].partsNum}`;
                    cell4.className = 'partsNum';

                    // 異なるテキストの長さに応じて文字サイズを調整
                    countLengthText(cell4);

                    cell5.innerHTML = `${supplys[i].requiredQuantity}`;
                    cell5.className = 'boxCount';

                    cell6.innerHTML = subResult;
                    cell6.setAttribute("data-time", dataTime); // 表示時間
                    cell6.setAttribute("data-countdown", supplys[i].countDownTime);
                    cell6.setAttribute("data-request-datetime", supplys[i].correctedRequestDatetime);
                    cell6.className = remainingMs > 0 ? 'timeCount' : 'timeCount redflag';

                    const machine = supplys[i].machineNum; //　機番
                    const hasReady = supplys[i].readyCount > 0;　//既に登録
                    let isReadyOrder = supplys[i].isReadyOrder;　// 準備フラグ
                    const isLastToComplete = (supplys[i].readyCount + 1) == supplys[i].totalCount;　//　完了前の最後の項目

                    if (!isReadyOrder && isLastToComplete && !renderedTotalButtons.has(machine)) {
                        //　全登録の最終行
                        cell7.innerHTML = `<button type="button" class="btn btn-primary btnTotalRegister">${supplys[i].displayNumber}</button>`;
                        cell9.innerHTML = `<button type="button" class="btn btn-secondary btnClose"><i class="fa-solid fa-xmark"></i></button>`;
                        renderedTotalButtons.add(machine); // この機番に集約ボタンが表示済みであることをマーク
                    }
                    else if (hasReady && isReadyOrder) {
                        // 登録済み』ボタン表示＋Closeボタン無効化
                        cell7.innerHTML = `<button type="button" class="btn btn-primary btnRegistered">${supplys[i].displayNumber}</button>`;
                        cell9.innerHTML = `<button type="button" class="btn btn-secondary btnCloseDisable"><i class="fa-solid fa-xmark"></i></button>`;
                    }
                    else {
                        // 未準備
                        cell7.innerHTML = `<button type="button" class="btn btn-primary btnRegister">${supplys[i].displayNumber}</button>`;
                        cell9.innerHTML = `<button type="button" class="btn btn-secondary btnClose"><i class="fa-solid fa-xmark"></i></button>`;
                    }

                    // ================================
                    cell8.innerHTML = `${supplys[i].partsSupplyRequestId}`;
                    cell8.className = 'supplyId';
                }
            } else {
                $(".supplyContent").hide();
                $(".noneDateMess").show();
            }
        }

        // 各tdを繰り返し、各tdにカウントダウン関数を適用する
        const tdElements = document.querySelectorAll('#tblSupplyLeft td.timeCount');
        const tdElements1 = document.querySelectorAll('#tblSupplyRight td.timeCount');
        tdElements.forEach(counttimer);
        tdElements1.forEach(counttimer);

        var btnRegister = document.querySelectorAll('.btnRegister');　//　登録
        var buttonsClose = document.querySelectorAll('.btnClose');　//　欠品
        var btnRegistered = document.querySelectorAll('.btnRegistered');　//　既に登録
        var btnTotalRegister = document.querySelectorAll('.btnTotalRegister');　//　全件登録

        btnRegister.forEach(btn => handleRegister(btn, true));　//　登録
        btnRegistered.forEach(btn => handleRegister(btn, false));　//　既に登録

        //　全件登録
        btnTotalRegister.forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.parentElement.parentElement;
                var tdWithSupplyId = row.querySelector('.supplyId');
                var tdWithMachineNumber = row.querySelector('.machine-number');
                var tdWithPartsNum = row.querySelector('.partsNum');

                // td要素のdata-timeとidを含むテキスト値を取得する
                var dataSupplyId = tdWithSupplyId.textContent;
                var dataMachineNumber = tdWithMachineNumber.textContent;
                var dataPartsNum = tdWithPartsNum.textContent;

                Swal.fire({
                    title: `依頼をすべて完了で登録してもよろしいですか？`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#0d6efd',
                    cancelButtonText: 'いいえ',
                    confirmButtonText: 'はい',
                    allowOutsideClick: false,
                }).then((result) => {
                    if (result.isConfirmed) {
                        $.ajax({
                            type: 'POST',
                            url: baseUrl + '/Parts/Complete',
                            data: { dataSupplyId: dataSupplyId, machineNum: dataMachineNumber },
                            success: function (response) {
                                if (response.res != true) {
                                    setTimeout(function () {
                                        Swal.fire({
                                            icon: 'error',
                                            title: `箱種 ${dataMachineNumber} 箱数 ${dataPartsNum}の<br>準備完了登録ができませんでした。<br>再度お試しください。`,
                                            html: `<span style="color: red;">${response.res}</span>`,
                                            confirmButtonColor: '#0d6efd',
                                            confirmButtonText: '閉じる',
                                            allowOutsideClick: false,
                                        });
                                    }, 500);
                                }
                            }
                        }).done(function () {
                            setTimeout(function () {
                                $("#overlay").fadeOut(300);
                            }, 500);
                        });
                    }
                });
            });
        });

        //　欠品
        buttonsClose.forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.parentElement.parentElement;
                var tdWithSupplyId = row.querySelector('.supplyId');
                var tdWithMachineNumber = row.querySelector('.machine-number');
                var tdWithPartsNum = row.querySelector('.partsNum');

                // td要素のdata-timeとidを含むテキスト値を取得する
                var dataSupplyId = tdWithSupplyId.textContent;
                var dataMachineNumber = tdWithMachineNumber.textContent;
                var dataPartsNum = tdWithPartsNum.textContent;

                Swal.fire({
                    title: '欠品の登録には職制のパスワードが必要です。',
                    input: 'password',
                    inputPlaceholder: 'パスワードを入力してください',
                    showCancelButton: true,
                    icon: 'warning',
                    cancelButtonText: 'キャンセル',
                    confirmButtonText: '欠品登録',
                    allowOutsideClick: false,
                    preConfirm: (value) => {
                        if (!value) {
                            Swal.showValidationMessage('パスワードを空にすることはできません');
                            return false;
                        }
                        return value;
                    }
                }).then((result) => {
                    if (result.isConfirmed) {
                        $.ajax({
                            type: 'POST',
                            url: baseUrl + '/Parts/RegisterOutOfStock',
                            data: { dataSupplyId: dataSupplyId, password: result.value },
                            success: function (response) {
                                if (response.res != true) {
                                    setTimeout(function () {
                                        Swal.fire({
                                            icon: 'error',
                                            title: `機番 ${dataMachineNumber} 所番地 ${dataPartsNum}の<br>の欠品登録ができませんでした。<br>再度お試しください。`,
                                            html: `<span style="color: red;">${response.errorMessage}</span>`,
                                            confirmButtonColor: '#0d6efd',
                                            confirmButtonText: '閉じる',
                                            allowOutsideClick: false,
                                        })
                                    }, 500);
                                }
                            }
                        }).done(function () {
                            setTimeout(function () {
                                $("#overlay").fadeOut(300);
                            }, 500);
                        });
                    }
                })
            });
        });
    }
}

//　登録検出
function handleRegister(button, isRegister) {
    button.addEventListener('click', function () {
        var row = button.parentElement.parentElement;
        var dataSupplyId = row.querySelector('.supplyId').textContent;
        var dataMachineNumber = row.querySelector('.machine-number').textContent;
        var dataPartsNum = row.querySelector('.partsNum').textContent;

        $.ajax({
            type: 'POST',
            url: baseUrl + '/Parts/Register',
            data: { dataSupplyId: dataSupplyId, isRegister: isRegister },
            success: function (response) {
                if (response.res != true) {
                    setTimeout(function () {
                        Swal.fire({
                            icon: 'error',
                            title: `機番 ${dataMachineNumber} 所番地 ${dataPartsNum}の<br>準備完了登録ができませんでした。<br>再度お試しください。`,
                            html: `<span style="color: red;">${response.res}</span>`,
                            confirmButtonColor: '#0d6efd',
                            confirmButtonText: '閉じる',
                            allowOutsideClick: false,
                        })
                    }, 500);
                }
            }
        }).done(function () {
            setTimeout(function () {
                $("#overlay").fadeOut(300);
            }, 500);
        });
    });
}
// ----------------------------------------------------------------------//


// -----------------------------------運搬画面-----------------------------------//
// SignalRを使用して接続を初期化する
const isTransportationPage = document.getElementById("transportation-page");
if (isTransportationPage) {
    var connectionTransport = new signalR.HubConnectionBuilder().withUrl("transportationHub").build();

    $(function () {
        connectionTransport.start().then(function () {
            InvokeTransports();
        })
    });

    // 短い遅延後に再接続を試みる
    var closeConnectTransportCount = 0;
    connectionTransport.onclose(function (error) {
        setTimeout(function () {
            connectionTransport.start().then(function () {
                InvokeTransports();
            })
            closeConnectTransportCount += 1;
            console.log("Error - onclose 再接続" + closeConnectTransportCount + "回目");
            // 接続が2回以上失われた場合はページをリロード
            if (closeConnectTransportCount >= 2)
                window.location.reload();
        }, 500);
    });

    // ページを離れた時やリロードしたタイミングで接続を止める
    window.addEventListener('unload', function () {
        console.log("addEventListener - unload");
        connectionTransport.stop();
    });

    // ハブのメソッドを呼び出す
    function InvokeTransports() {
        connectionTransport.invoke("SendTransportations").catch(function (error) {
            // Controllerに接続できない場合はエラー
            console.log("Error - invoke catch");
            $(".connectionTransportError").text(error);
            $(".connectionTransportError").show();
        });
    }

    // エラー発生時
    connectionTransport.on("Error", (error) => {
        console.log("Error - on");
        $(".connectionTransportError").text(error);
        $(".connectionTransportError").show();
    });

    // グリッドに依頼をバインドする
    connectionTransport.on("ReceivedTransportations", function (products) {
        BindTransportsToGrid(products);
    });
}

// グリッドに依頼をバインド
function BindTransportsToGrid(transports) {
    $('#tblTransportLeft tbody').empty();
    $('#tblTransportRight tbody').empty();

    var tableLeftDom = document.getElementById('tblTransportLeft');
    var tableRightDom = document.getElementById('tblTransportRight');

    if (tableLeftDom !== null) {
        var table = tableLeftDom.getElementsByTagName('tbody')[0];
        var table1 = tableRightDom.getElementsByTagName('tbody')[0];

        // 左のテーブル取得
        var supplys1 = transports.slice(0, 6);
        createTable(supplys1, table);

        // 右のテーブル取得
        var supplysTmp = transports.length - supplys1.length;
        if (supplysTmp > 0) {
            var supplys2 = transports.slice(6, 12);
            createTable(supplys2, table1);
        }

        // テーブルを作成
        function createTable(transports, table) {
            if (transports.length > 0) {
                $(".transportContent").show();
                $(".noneDateMess").hide();

                for (let i = 0; i < transports.length; i++) {
                    var row = table.insertRow();
                    var cell1 = row.insertCell(0);
                    var cell2 = row.insertCell(1);
                    var cell3 = row.insertCell(2);
                    var cell4 = row.insertCell(3);
                    var cell5 = row.insertCell(4);
                    var cell6 = row.insertCell(5);
                    var cell7 = row.insertCell(6);
                    var cell8 = row.insertCell(7);

                    // 現在日時
                    var today = new Date();

                    // 依頼時間
                    var resDateTime = new Date(transports[i].correctedRequestDatetime);
                    var elapsedMs = today - resDateTime;　// 残り時間
                    var countdownMs = transports[i].countDownTime * 60000;　//カウント時間
                    var remainingMs = countdownMs - elapsedMs;　// 経過時間
                    var dataTime = elapsedMs;
                    var subResult;

                    if (elapsedMs >= 60 * 60000) { // 1時間経過した場合、表示を「59:59」で固定
                        subResult = "59:59";
                    } else if (remainingMs > 0) { // カウントダウン中（残り時間がまだある）
                        const minutes = Math.floor(remainingMs / 60000);
                        const seconds = Math.floor((remainingMs % 60000) / 1000);
                        subResult = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
                    } else { // カウントアップ中（カウントダウン終了後の経過時間を表示）
                        const totalMinutes = transports[i].countDownTime + Math.floor((elapsedMs - countdownMs) / 60000);
                        const seconds = Math.floor((elapsedMs % 60000) / 1000);
                        const displayMinutes = Math.min(totalMinutes, 59);
                        subResult = `${displayMinutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
                    }

                    cell2.innerHTML = `${transports[i].machineNum}`;

                    //cell3.innerHTML = `${transports[i].boxType}`;
                    cell3.innerHTML = `${transports[i].supplyLocation}`;
                    cell3.className = 'boxType';

                    //cell4.innerHTML = `${transports[i].boxType}`;
                    cell4.innerHTML = `${transports[i].partsNum}`;
                    cell4.className = 'boxType';

                    // 異なるテキストの長さに応じて文字サイズを調整
                    countLengthText(cell3);

                    cell5.innerHTML = `${transports[i].requiredQuantity}`;
                    cell5.className = 'boxCount';
                    cell7.className = 'statusBtn';

                    if (transports[i].transportationStartDatetime == null && transports[i].transportationEndDatetime == null) {
                        cell7.innerHTML = `<button type="button" class="btn btn-warning btnRegister">開始</button>`;
                    } else if (transports[i].transportationStartDatetime != null && transports[i].transportationEndDatetime == null) { 
                        cell7.innerHTML = `<button type="button" class="btn btn-success btnRegister btnEnd">終了</button>`;
                    }
                    cell6.innerHTML = subResult;
                    cell6.setAttribute("data-time", dataTime); // 表示時間
                    cell6.setAttribute("data-countdown", transports[i].countDownTime);
                    cell6.setAttribute("data-request-datetime", transports[i].correctedRequestDatetime);
                    cell6.className = remainingMs > 0 ? 'timeCount' : 'timeCount redflag';

                    cell8.innerHTML = `${transports[i].emptyBoxSupplyRequestId}`;
                    cell8.className = 'transportId';
                }
            } else {
                $(".transportContent").hide();
                $(".noneDateMess").show();
            }
        }

        // 各tdを繰り返し、各tdにカウントダウン関数を適用する
        const tdElements = document.querySelectorAll('#tblTransportLeft td.timeCount');
        const tdElements1 = document.querySelectorAll('#tblTransportRight td.timeCount');
        tdElements.forEach(counttimer);
        tdElements1.forEach(counttimer);

        var buttons = document.querySelectorAll('.btnRegister');
        var buttonsClose = document.querySelectorAll('.btnClose');

        // ボタン押下時に確認ダイアログ表示
        buttons.forEach(function (button) {
            button.addEventListener('click', function () {
                var isCancelled = false;
                var row = button.parentElement.parentElement;
                var statusBtn = button.textContent || button.innerText;
                var tdWithSupplyId = row.querySelector('.transportId');
                var tdWithBoxType = row.querySelector('.boxType');
                var tdWithBoxCount = row.querySelector('.boxCount');

                // td要素のdata-timeとidを含むテキスト値を取得
                var dataSupplyId = tdWithSupplyId.textContent;
                var dataBoxType = tdWithBoxType.textContent;
                var dataBoxCount = tdWithBoxCount.textContent;

                // 開始ボタン押下時
                if (statusBtn == "開始") {
                    $.ajax({
                        type: 'POST',
                        url: baseUrl + '/Transportation/Complete',
                        data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isCancelled: isCancelled },
                        success: function (response) {
                            if (response.res == true) {
                                button.innerText = "終了";
                                button.classList.remove('btn-warning');
                                button.classList.add('btn-success');
                            } else {
                                setTimeout(function () {
                                    Swal.fire({
                                        icon: 'error',
                                        title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>運搬開始登録ができませんでした。<br>再度お試しください。`,
                                        html: `<span style="color: red;">${response.res}</span>`,
                                        confirmButtonColor: '#0d6efd',
                                        confirmButtonText: '閉じる',
                                        allowOutsideClick: false,
                                    })
                                }, 500);
                            }
                        }
                    }).done(function () {
                        setTimeout(function () {
                            $("#overlay").fadeOut(300);
                        }, 10);
                    });
                }

                // 終了ボタン押下時
                if (statusBtn == "終了") {
                    Swal.fire({
                        title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>運搬終了登録を行います。<br>よろしいですか？`,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#198754',
                        cancelButtonText: 'キャンセル',
                        allowOutsideClick: false,
                        confirmButtonText: '終了'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            $.ajax({
                                type: 'POST',
                                url: baseUrl + '/Transportation/Complete',
                                data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isCancelled: isCancelled },
                                success: function (response) {
                                    if (response.res != true) {
                                        setTimeout(function () {
                                            Swal.fire({
                                                icon: 'error',
                                                title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>運搬終了登録ができませんでした。<br>再度お試しください。`,
                                                html: `<span style="color: red;">${response.res}</span>`,
                                                confirmButtonColor: '#0d6efd',
                                                confirmButtonText: '閉じる',
                                                allowOutsideClick: false,
                                            })
                                        }, 500);
                                    }
                                }
                            }).done(function () {
                                setTimeout(function () {
                                    $("#overlay").fadeOut(300);
                                }, 500);
                            });
                        }
                    })
                }
            });
        });

        // ボタン押下時に確認ダイアログ表示
        buttonsClose.forEach(function (button) {
            button.addEventListener('click', function () {
                var isCancelled = true;
                var row = button.parentElement.parentElement;
                var tdWithSupplyId = row.querySelector('.transportId');
                var tdWithBoxType = row.querySelector('.boxType');
                var tdWithBoxCount = row.querySelector('.boxCount');
                var tdWithbtnRegister = row.querySelector('.btnRegister');

                // td要素のdata-timeとidを含むテキスト値を取得する
                var dataSupplyId = tdWithSupplyId.textContent;
                var dataBoxType = tdWithBoxType.textContent;
                var dataBoxCount = tdWithBoxCount.textContent;
                var statusBtn = tdWithbtnRegister.textContent || tdWithbtnRegister.innerText;

                // 開始ボタン隣の削除ボタン押下時
                if (statusBtn == "開始") {
                    $.ajax({
                        type: 'POST',
                        url: baseUrl + '/Transportation/Complete',
                        data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isCancelled: isCancelled },
                        success: function (response) {
                            if (response.res == true) {
                                tdWithbtnRegister.innerText = "開始";
                                tdWithbtnRegister.classList.add('btn-warning');
                                tdWithbtnRegister.classList.remove('btn-success');
                            } else {
                                setTimeout(function () {
                                    Swal.fire({
                                        icon: 'error',
                                        title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>準備完了の取消ができませんでした。<br>再度お試しください。`,
                                        html: `<span style="color: red;">${response.res}</span>`,
                                        confirmButtonColor: '#0d6efd',
                                        confirmButtonText: '閉じる',
                                        allowOutsideClick: false,
                                    })
                                }, 500);
                            }
                        }
                    }).done(function () {
                        setTimeout(function () {
                            $("#overlay").fadeOut(300);
                        }, 10);
                    });
                }

                // 終了ボタン隣の削除ボタン押下時
                if (statusBtn == "終了") {
                    $.ajax({
                        type: 'POST',
                        url: baseUrl + '/Transportation/Complete',
                        data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isCancelled: isCancelled },
                        success: function (response) {
                            if (response.res == true) {
                                tdWithbtnRegister.innerText = "開始";
                                tdWithbtnRegister.classList.add('btn-warning');
                                tdWithbtnRegister.classList.remove('btn-success');
                            } else {
                                setTimeout(function () {
                                    Swal.fire({
                                        icon: 'error',
                                        title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>運搬開始の取消ができませんでした。<br>再度お試しください。`,
                                        html: `<span style="color: red;">${response.res}</span>`,
                                        confirmButtonColor: '#0d6efd',
                                        confirmButtonText: '閉じる',
                                        allowOutsideClick: false,
                                    })
                                }, 500);
                            }
                        }
                    }).done(function () {
                        setTimeout(function () {
                            $("#overlay").fadeOut(300);
                        }, 10);
                    });
                }
            });
        });
    }
}
// ----------------------------------------------------------------------//


// 異なるテキストの長さに応じて文字サイズを調整
function countLengthText(cell3) {
    var fullwidthCount = cell3.innerText.length;
    if (fullwidthCount == 9)
        cell3.style.fontSize = 23 + 'px';
    else if (fullwidthCount == 8)
        cell3.style.fontSize = 25 + 'px';
    else if (fullwidthCount == 7)
        cell3.style.fontSize = 27 + 'px';
    else if (fullwidthCount == 6)
        cell3.style.fontSize = 28 + 'px';
    else if (fullwidthCount == 5)
        cell3.style.fontSize = 29 + 'px';
    else if (fullwidthCount <= 4)
        cell3.style.fontSize = 30 + 'px';
}

// data-time を毎秒更新
function counttimer(element) {
    // サーバーから元のデータを取得
    if (element.dataset.timerStarted === "true") return;
    element.dataset.timerStarted = "true";

    const requestTime = new Date(element.getAttribute('data-request-datetime')).getTime();
    const countdownMinutes = parseInt(element.getAttribute("data-countdown"), 10) || 0;
    const countdownMs = countdownMinutes * 60000;

    const interval = setInterval(() => {
        const now = Date.now();
        // 経過時間を計算する（リクエストから現在まで）
        const elapsedMs = now - requestTime;
        //  残り時間（マイナスになる場合もあります）
        const remainingMs = countdownMs - elapsedMs;

        // data-time は経過した時間（経過時間）を保持
        const dataTime = elapsedMs;
        element.setAttribute("data-time", dataTime);
        if (elapsedMs >= 60 * 60000) {
            element.textContent = "59:59";
            element.className = 'timeCount redflag';
            clearInterval(interval);
            return;
        }

        let displayText;
        // ステータスを決定
        if (remainingMs > 0) {　// カウントダウン中（残り時間がまだある場合）
            const minutes = Math.floor(remainingMs / 60000);
            const seconds = Math.floor((remainingMs % 60000) / 1000);
            displayText = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
            element.className = 'timeCount';
        } else {　// カウントアップ中（カウントダウン終了後）
            const overMs = elapsedMs - countdownMs;
            const totalMinutes = countdownMinutes + Math.floor(overMs / 60000);
            const seconds = Math.floor((overMs % 60000) / 1000);
            const displayMinutes = Math.min(totalMinutes, 59);

            if (totalMinutes >= 59 && seconds >= 59) {　// 59:59 に到達した場合
                displayText = "59:59";
                element.className = 'timeCount redflag';
                clearInterval(interval);
            } else {　// 通常のカウントアップ表示
                displayText = `${displayMinutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
                element.className = 'timeCount redflag';
            }
        }

        element.textContent = displayText;
    }, 1000);
}

// ==================== 自動並び替え ==================== //


//　行を並べる
function sortTableRows() {
    // 供給ページか輸送ページかを判定
    const isSupplyPage = !!document.querySelector("#tblSupplyLeft");
    const isTransportPage = !!document.querySelector("#tblTransportLeft");
    if (!isSupplyPage && !isTransportPage) return;　// 対象ページでなければ処理しない

    // 左右のテーブルIDをページ種別に応じて設定
    const leftTableId = isSupplyPage ? "#tblSupplyLeft" : "#tblTransportLeft";
    const rightTableId = isSupplyPage ? "#tblSupplyRight" : "#tblTransportRight";
    // 左右テーブルの tbody を取得
    const leftTbody = document.querySelector(`${leftTableId} tbody`);
    const rightTbody = document.querySelector(`${rightTableId} tbody`);
    if (!leftTbody || !rightTbody) return;　// tbody が存在しなければ終了

    // 左右両方のテーブルからすべての行（tr）を取得
    const allRows = Array.from(document.querySelectorAll(`${leftTableId} tbody tr, ${rightTableId} tbody tr`));
    if (allRows.length === 0) return;

    // 並び替え用に各行データをパース
    const parsedRows = allRows.map(row => {
        const tdTime = row.querySelector(".timeCount");　// 時間表示セル
        const tdId = row.querySelector(isSupplyPage ? ".supplyId" : ".transportId");　// IDセル
        const timeText = tdTime?.textContent.trim() || "00:00";　// 表示時間
        const isCountUp = tdTime?.classList.contains("redflag"); // redflag = カウントアップ表示
        const id = parseInt(tdId?.textContent || "0", 10);　// ID 数値変換
        return { row, timeText, isCountUp, id };　// 並び替えに必要な情報を返す
    });


    // 並べる処理
    parsedRows.sort((a, b) => {
        // 「59:59」を最優先で先頭に並べる
        if (a.timeText === "59:59" && b.timeText !== "59:59") return -1;
        if (b.timeText === "59:59" && a.timeText !== "59:59") return 1;

        // カウントアップをカウントダウンより優先して前に並べる
        if (a.isCountUp && !b.isCountUp) return -1;
        if (!a.isCountUp && b.isCountUp) return 1;

        // 同じグループ内では時間を比較して並べる
        const timeA = a.timeText.split(':').map(Number);
        const timeB = b.timeText.split(':').map(Number);
        const totalA = timeA[0] * 60 + timeA[1];
        const totalB = timeB[0] * 60 + timeB[1];

        if (a.isCountUp) {
            // カウントアップ：時間の大きい順（降順）
            if (totalA > totalB) return -1;
            if (totalA < totalB) return 1;
        } else {
            // カウントダウン：時間の小さい順（昇順）
            if (totalA < totalB) return -1;
            if (totalA > totalB) return 1;
        }

        // 同値なら ID が大きい順
        return b.id - a.id;
    });

    leftTbody.innerHTML = "";
    rightTbody.innerHTML = "";
    parsedRows.forEach((item, index) => {
        if (index < 6) leftTbody.appendChild(item.row);
        else rightTbody.appendChild(item.row);
    });
}

setInterval(sortTableRows, 1000);

// ローディング表示
$(document).ajaxSend(function () {
    $("#overlay").fadeIn();
});

// 文字列の全角の長さを計算
function countFullwidthCharacters(str) {
    return Array.from(str).reduce(function (count, char) {
        return count + (char.match(/[^\x00-\x7F]/) ? 2 : 1);
    }, 0);
}

// 文字列の半角の長さを計算
function countHalfwidthCharacters(str) {
    return Array.from(str).reduce(function (count, char) {
        return count + (char.match(/[^\x00-\xFF]/) ? 1 : 0);
    }, 0);
}

// ページが完全にロードされるまでローディング表示
document.addEventListener("DOMContentLoaded", function () {
    $("#overlay").fadeIn();

    window.addEventListener("load", function () {
        $("#overlay").fadeOut();
    });
});


// ----------------------------------------------------------------------//


// -----------------------------------稼働状況-----------------------------------//
// SignalRを使用して接続を初期化する
const isMachinePage = document.getElementById("machine-page");
if (isMachinePage) {
    var connectionMachine = new signalR.HubConnectionBuilder().withUrl("machineHub").build();

    $(function () {
        connectionMachine.start().then(function () {
            InvokeMachines();
        })
    });

    // ページを離れた時やリロードしたタイミングで接続を止める
    window.addEventListener('unload', function () {
        console.log("addEventListener - unload");
        connectionMachine.stop();
    });

    // ハブのメソッドを呼び出す
    function InvokeMachines() {
        //  URLからDivisionを取得
        connectionMachine.invoke("SendMachineStatusList").catch(function (error) {
            // Controllerに接続できない場合はエラー
            console.log("Error - invoke catch");
            $(".connectionTransportError").text(error);
            $(".connectionTransportError").show();
        });
    }

    // エラー発生時
    connectionMachine.on("Error", (error) => {
        console.log("Error - on");
        $(".connectionTransportError").text(error);
        $(".connectionTransportError").show();
    });

    // グリッドに依頼をバインドする
    connectionMachine.on("ReceivedMachineStatusList", function (machines) {
        const queryParam = new URLSearchParams(window.location.search).get("zone");
        var filterMachines = machines.filter((item) => item.zone == queryParam);
        BindMachinesToGrid(filterMachines);
    });
}

// グリッドに依頼をバインド
function BindMachinesToGrid(machines) {
    $('#machineTable tbody').empty();

    var machineDom = document.getElementById('machineTable');

    if (machineDom !== null) {
        var table = machineDom.getElementsByTagName('tbody')[0];

        // テーブル取得
        createTable(machines, table);

        // テーブルを作成
        function createTable(machines, table) {
            if (machines.length > 0) {
                $(".machine-table-content").show();
                $(".noneDateMess").hide();

                for (let i = 0; i < machines.length; i++) {
                    var row = table.insertRow();
                    var cell1 = row.insertCell(0);
                    var cell2 = row.insertCell(1);
                    var cell3 = row.insertCell(2);

                    cell1.innerHTML = `${machines[i].machineNum}`;
                    cell2.innerHTML = `${machines[i].status ? '稼働中' : '停止'}`;
                    cell3.innerHTML = !isDotNetMinDate(machines[i].endTime) ? `${formatDate(machines[i].endTime)}` : "-";
                    if (machines[i].status) {
                        cell2.style.color = 'orange';
                    } else {
                        cell2.style.color = 'green';
                    }
                }
            } else {
                $(".machine-table-content").hide();
                $(".noneDateMess").show();
            }
        }
        function isDotNetMinDate(value) {
            return value === "0001-01-01T00:00:00" ||
                value.startsWith("0001-01-01");
        }
    }
}
function formatDate(dateInput) {
    const date = new Date(dateInput);
    if (isNaN(date)) return "";
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0'); // Months start at 0
    const dd = String(date.getDate()).padStart(2, '0');
    const hh = String(date.getHours()).padStart(2, '0');
    const min = String(date.getMinutes()).padStart(2, '0');

    return `${yyyy}/${mm}/${dd} ${hh}:${min}`;
}
