// -----------------------------------準備画面-----------------------------------//

var baseSupplyUrl = window.location.origin;
var supplyRelativePath = "/supplyHub";
var supplyApiUrl = baseSupplyUrl + supplyRelativePath;

// SignalRを使用して接続を初期化する
//var connectionSupply = new signalR.HubConnectionBuilder().withUrl("/supplyHub").build();
var connectionSupply = new signalR.HubConnectionBuilder().withUrl(supplyApiUrl).build();

$(function () {
    connectionSupply.start().then(function () {
        InvokeSupplys();
    }).catch(function (err) {
        $(".connectionError").show();
    });
});

// 短い遅延後に再接続を試みる
connectionSupply.onclose(function (error) {
    setTimeout(function () {
        connectionSupply.start().catch(function (err) {
            $(".connectionError").show();
        });
    }, 500);
});

// ハブのメソッドを呼び出す
function InvokeSupplys() {
    connectionSupply.invoke("SendSupplys").catch(function (err) {
        $(".connectionError").show();
    });
}

// グリッドに依頼をバインドする
connectionSupply.on("ReceivedSupplys", function (supplys) {
    BindSupplysToGrid(supplys);
});

var boxTypeLength = $(".boxType");

// グリッドに依頼をバインドする
function BindSupplysToGrid(supplys) {
    $('#tblSupplyLeft tbody').empty();
    $('#tblSupplyRight tbody').empty();

    var tableLeftDom = document.getElementById('tblSupplyLeft');
    var tableRightDom = document.getElementById('tblSupplyRight');

    if (tableLeftDom !== null) {
        var table = tableLeftDom.getElementsByTagName('tbody')[0];
        var table1 = tableRightDom.getElementsByTagName('tbody')[0];

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

                    // 現在日時
                    var today = new Date();
                    // 依頼時間
                    var resDateTime = new Date(supplys[i].correctedRequestDatetime);

                    // 2つの日付のミリ秒単位の差を計算
                    var differenceTime = Math.abs(today - resDateTime);

                    // ミリ秒から時間、分変換
                    var minuteNum = Math.floor(differenceTime / (1000 * 60));
                    var secondNum = Math.floor((differenceTime % (1000 * 60)) / 1000);

                    // mm:ss表記に変換
                    var subResult = "";
                    //　60:00以上はデフォルト"59:59"
                    if (minuteNum >= 60)
                        subResult = "59:59";
                    else
                        subResult = subtractTime(today, resDateTime, subResult);　// 時間を引く

                    var timeTmp = subResult.split(':');
                    var dataTime = ((parseInt(timeTmp[0]) * 60) + (parseInt(timeTmp[1]))) * 1000;

                    if (supplys[i].important == 1) {
                        //cell1.innerHTML = `<p class="importantFlag">特急</p>`;
                        cell1.className = 'importTd';
                    }

                    cell2.innerHTML = `${supplys[i].machineNum}`;
                    cell2.className = 'machine-number';
                    cell3.innerHTML = `${supplys[i].permanentAbbreviation}`;
                    cell4.innerHTML = `${supplys[i].boxType}`;
                    cell4.className = 'boxType';

                    // 異なるテキストの長さに応じて文字サイズを調整
                    var cellTextLength = cell4.innerText.length;
                    var minFontSize = 12;
                    var maxFontSize = 30;
                    var fontSize = maxFontSize - (cellTextLength * 2);
                    fontSize = Math.max(minFontSize, Math.min(maxFontSize, fontSize));
                    cell4.style.fontSize = fontSize + 'px';

                    cell5.innerHTML = `${supplys[i].boxCount}`;
                    cell5.className = 'boxCount';
                    cell6.innerHTML = `${subResult}`;
                    cell7.innerHTML = `<button type="button" class="btn btn-primary btnCompletion">完了</button>`;

                    if (dataTime < 0) {
                        row.className = "redBg";
                        cell6.className = 'timeCount redflag';
                    }
                    else
                        cell6.className = 'timeCount';

                    cell6.setAttribute("data-time", dataTime);
                    cell8.innerHTML = `${supplys[i].emptyBoxSupplyRequestId}`;
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

        var buttons = document.querySelectorAll('.btnCompletion');

        // ボタン押下時に確認ダイアログ表示
        buttons.forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.parentElement.parentElement;
                var tdWithSupplyId = row.querySelector('.supplyId');
                var tdWithBoxType = row.querySelector('.boxType');
                var tdWithBoxCount = row.querySelector('.boxCount');

                // td要素のdata-timeとidを含むテキスト値を取得する
                var dataSupplyId = tdWithSupplyId.textContent;
                var dataBoxType = tdWithBoxType.textContent;
                var dataBoxCount = tdWithBoxCount.textContent;

                // sweetalert2
                Swal.fire({
                    title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>準備完了登録を行います。<br>よろしいですか？`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#0d6efd',
                    cancelButtonText: 'キャンセル',
                    confirmButtonText: '完了'
                }).then((result) => {
                    if (result.isConfirmed) {
                        $.ajax({
                            type: 'POST',
                            url: baseSupplyUrl +'/Supply/Complete',
                            data: { dataSupplyId: dataSupplyId },
                            success: function (response) {
                                console.log(response)
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
// ----------------------------------------------------------------------//


// -----------------------------------運搬画面-----------------------------------//
var baseTransportUrl = window.location.origin;
var transportRelativePath = "/transportHub";
var transportApiUrl = baseTransportUrl + transportRelativePath;

// SignalRを使用して接続を初期化する
var connectionTransport = new signalR.HubConnectionBuilder().withUrl(transportApiUrl).build();
$(function () {
    connectionTransport.start().then(function () {
        InvokeTransports();
    }).catch(function (err) {
        return console.error(err.toString());
    });
});

// 短い遅延後に再接続を試みる
connectionTransport.onclose(function (error) {
    setTimeout(function () {
        connectionTransport.start().catch(function (err) {
            console.error(err.toString());
        });
    }, 500);
});

// ハブのメソッドを呼び出す
function InvokeTransports() {
    connectionTransport.invoke("SendTransports").catch(function (err) {
        return console.error(err.toString());
    });
}

// グリッドに依頼をバインドする
connectionTransport.on("ReceivedTransports", function (products) {
    BindTransportsToGrid(products);
});

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
                    var cell9 = row.insertCell(8);

                    // 現在日時
                    var today = new Date();
                    // 依頼時間
                    var resDateTime = new Date(transports[i].correctedRequestDatetime);

                    // 2つの日付のミリ秒単位の差を計算
                    var differenceTime = Math.abs(today - resDateTime);

                    // ミリ秒から時間、分変換
                    var minuteNum = Math.floor(differenceTime / (1000 * 60));
                    var secondNum = Math.floor((differenceTime % (1000 * 60)) / 1000);

                    // mm:ss表記に変換
                    var subResult = "";
                    // 60:00以上はデフォルト"59:59"
                    if (minuteNum >= 60)
                        subResult = "59:59";
                    else
                        subResult = subtractTime(today, resDateTime, subResult);　// 時間を引く

                    var timeTmp = subResult.split(':');
                    var dataTime = ((parseInt(timeTmp[0]) * 60) + (parseInt(timeTmp[1]))) * 1000;

                    cell1.className = 'importTd';
                    cell2.innerHTML = `${transports[i].machineNum}`;
                    cell2.className = 'machine-number';
                    cell3.innerHTML = `${transports[i].permanentAbbreviation}`;
                    cell4.innerHTML = `${transports[i].boxType}`;
                    cell4.className = 'boxType';

                    // 異なるテキストの長さに応じて文字サイズを調整
                    var cellTextLength = cell4.innerText.length;
                    var minFontSize = 12;
                    var maxFontSize = 30;
                    var fontSize = maxFontSize - (cellTextLength * 2);
                    fontSize = Math.max(minFontSize, Math.min(maxFontSize, fontSize));
                    cell4.style.fontSize = fontSize + 'px';

                    cell5.innerHTML = `${transports[i].boxCount}`;
                    cell5.className = 'boxCount';
                    cell6.innerHTML = `${subResult}`;
                    cell7.className = 'statusBtn';

                    if (transports[i].emptyBoxSupplyStatusId == 2)
                        cell7.innerHTML = `<button type="button" class="btn btn-warning btnRegister">開始</button>`;

                    if (transports[i].emptyBoxSupplyStatusId == 3)
                        cell7.innerHTML = `<button type="button" class="btn btn-success btnRegister btnEnd">終了</button>`;

                    if (dataTime < 0) {
                        row.className = "redBg";
                        cell6.className = 'timeCount redflag';
                    }
                    else
                        cell6.className = 'timeCount';

                    cell6.setAttribute("data-time", dataTime);
                    cell8.innerHTML = `${transports[i].emptyBoxSupplyRequestId}`;
                    cell8.className = 'transportId';
                    cell9.innerHTML = `<button type="button" class="btn btn-secondary btnClose"><i class="fa fa-window-close">X</i></button>`;
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
                var isDelete = false;
                var row = button.parentElement.parentElement;
                var statusBtn = button.textContent || button.innerText;
                var tdWithSupplyId = row.querySelector('.transportId');
                var tdWithBoxType = row.querySelector('.boxType');
                var tdWithBoxCount = row.querySelector('.boxCount');

                // td要素のdata-timeとidを含むテキスト値を取得
                var dataSupplyId = tdWithSupplyId.textContent;
                var dataBoxType = tdWithBoxType.textContent;
                var dataBoxCount = tdWithBoxCount.textContent;

                if (statusBtn == "開始") {
                    $.ajax({
                        type: 'POST',
                        url: baseTransportUrl + '/Transport/Complete',
                        data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isDelete: isDelete },
                        success: function (response) {
                            if (response.res == true) {
                                button.innerText = "終了";
                                button.classList.remove('btn-warning');
                                button.classList.add('btn-success');
                            }
                        }
                    }).done(function () {
                        setTimeout(function () {
                            $("#overlay").fadeOut(300);
                        }, 10);
                    });
                }

                if (statusBtn == "終了") {
                    // sweetalert2
                    Swal.fire({
                        title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>運搬終了登録を行います。<br>よろしいですか？`,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#198754',
                        cancelButtonText: 'キャンセル',
                        confirmButtonText: '完了'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            $.ajax({
                                type: 'POST',
                                url: baseTransportUrl + '/Transport/Complete',
                                data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isDelete: isDelete },
                                success: function (response) {
                                    console.log(response)
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
                var isDelete = true;
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
                if (statusBtn == "開始") {
                    $.ajax({
                        type: 'POST',
                        url: baseTransportUrl + '/Transport/Complete',
                        data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isDelete: isDelete },
                        success: function (response) {
                            if (response.res == true) {
                                tdWithbtnRegister.innerText = "開始";
                                tdWithbtnRegister.classList.add('btn-warning');
                                tdWithbtnRegister.classList.remove('btn-success');
                            }
                        }
                    }).done(function () {
                        setTimeout(function () {
                            $("#overlay").fadeOut(300);
                        }, 10);
                    });
                }

                if (statusBtn == "終了") {
                    $.ajax({
                        type: 'POST',
                        url: baseTransportUrl + '/Transport/Complete',
                        data: { dataSupplyId: dataSupplyId, statusBtn: statusBtn, isDelete: isDelete },
                        success: function (response) {
                            if (response.res == true) {
                                tdWithbtnRegister.innerText = "開始";
                                tdWithbtnRegister.classList.add('btn-warning');
                                tdWithbtnRegister.classList.remove('btn-success');
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

// カウントタイマー
function counttimer(element) {
    // 表示されている時間を取得
    const startTime = parseInt(element.getAttribute('data-time'), 10);
    let timeLeft = startTime;
    let isCountingDown = true;

    // カウントダウン・カウントアップを開始する
    let countdownInterval;
    let countupInterval;

    // 経過時間が59:59でない場合はカウントダウンまたはカウントアップ
    if (timeLeft != 3599000) { // 3599000 (00:59:59)
        // 経過時間が10分を超える場合はカウントアップ
        if (timeLeft < 0) {
            timeLeft = -1 * timeLeft + 10 * 60000;
            isCountingDown = false;
        }

        // 時間表示(mm:ss)を更新
        updateDisplay(element, timeLeft);

        // 経過時間が10分を超えていない場合はカウントダウン
        if (timeLeft > 0 && isCountingDown) {
            // 1秒おきに更新
            countdownInterval = setInterval(function () {
                // 1秒ずつ減算
                if (timeLeft > 0)
                    timeLeft -= 1000;

                // 時間表示(mm:ss)を更新
                updateDisplay(element, timeLeft);

                // 経過時間が10分以上になったら、カウントアップに変更
                if (timeLeft <= 0) {
                    clearInterval(countdownInterval);

                    // 初期の時間から再びカウントダウンを開始する
                    timeLeft = startTime;
                    isCountingDown = false;
                    timeLeft = 10 * 60000;

                    // 赤字で1秒ずつ加算
                    countupInterval = setInterval(function () {
                        timeLeft += 1000;
                        element.className = 'timeCount redflag'

                        // 59:59になったらクリア
                        if (timeLeft == 3599000)
                            clearInterval(countupInterval);

                        // 時間表示(mm:ss)を更新
                        updateDisplay(element, timeLeft);
                    }, 1000);
                }
            }, 1000);

        // 経過時間が10分を超えている場合はカウントアップ
        } else {
            isCountingDown = false;

            // 1秒おきに更新
            countupInterval = setInterval(function () {
                // 赤字で1秒ずつ加算
                timeLeft += 1000;
                element.className = 'timeCount redflag'

                // 59:59になったらクリア
                if (timeLeft == 3599000)
                    clearInterval(countupInterval);

                // 時間表示(mm:ss)を更新
                updateDisplay(element, timeLeft);
            }, 1000);
        }
    } else {
        // 59:59の場合は赤字表示
        countupInterval = setInterval(function () {
            element.className = 'timeCount redflag'
            // 時間表示(mm:ss)を更新
            updateDisplay(element, timeLeft);
        }, 10);
    }
}

// 現在時刻から依頼日時を引き、HH:mm:ssに変換
function subtractTime(today, resDateTime, subResult) {
    var differenceTime = Math.abs(today - resDateTime);

    // ミリ秒から時間、分、秒に変換する
    var minuteResult = Math.floor((differenceTime % (1000 * 60 * 60)) / (1000 * 60));
    var secondResult = Math.floor((differenceTime % (1000 * 60)) / 1000);

    // 結果を 'HH:mm:ss' 形式の文字列にフォーマットする
    subResult = `${(10 - minuteResult).toString().padStart(2, '0')}:${(0 - secondResult.toString().padStart(2, '0'))}`;
    return subResult;
}

// カウントアップ時に背景を変更
setInterval(function () {
    var tdElements = document.querySelectorAll('td.redflag');
    tdElements.forEach(function (tdElement) {
        var trElement = tdElement.parentElement;
        trElement.style.backgroundColor = '#FFC1C1';
    });
}, 10);

// ローディング表示
$(document).ajaxSend(function () {
    $("#overlay").fadeIn(300);
});

// 時間表示(mm:ss)を更新
function updateDisplay(element, time) {
    var minutes = Math.floor(time / 60000);
    if (minutes >= 60)
        minutes = 0;
    var seconds = Math.floor((time % 60000) / 1000);
    element.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
}
