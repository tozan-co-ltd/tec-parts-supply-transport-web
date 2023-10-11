"use strict";

// signalRを使用して接続を初期化する
var connection = new signalR.HubConnectionBuilder().withUrl("/supplyHub").build();
$(function () {
    connection.start().then(function () {
        InvokeSupplys();
    }).catch(function (err) {
        return console.error(err.toString());
    });
});

// 短い遅延後に再接続を試みる
connection.onclose(function (error) {
    setTimeout(function () {
        connection.start().catch(function (err) {
            console.error(err.toString());
        });
    }, 500); 
});

// InvokeSupplys
function InvokeSupplys() {
    connection.invoke("SendSupplys").catch(function (err) {
        return console.error(err.toString());
    });
}

// グリッドにサプライをバインドする
connection.on("ReceivedSupplys", function (products) {
    BindSupplysToGrid(products);
});

// グリッドにサプライをバインド
function BindSupplysToGrid(supplys) {
    $('#tblSupplyLeft tbody').empty();
    $('#tblSupplyRight tbody').empty();

    var table = document.getElementById('tblSupplyLeft').getElementsByTagName('tbody')[0];
    var table1 = document.getElementById('tblSupplyRight').getElementsByTagName('tbody')[0];

    // 左のテーブル用を取得
    var supplys1 = supplys.slice(0, 6);
    createTable(supplys1, table);

    //右のテーブル用を取得
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
                //　依頼時間
                var resDateTime = new Date(supplys[i].correctedRequestDatetime);

                // 二つの日付のミリ秒単位の差を計算する
                var differenceTime = Math.abs(today - resDateTime);

                // ミリ秒から時間、分変換する
                var minuteNum = Math.floor(differenceTime / (1000 * 60));
                var secondNum = Math.floor((differenceTime % (1000 * 60)) / 1000);

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

    // カウントダウン
    function countdown(element) {
        const startTime = parseInt(element.getAttribute('data-time'), 10);
        let timeLeft = startTime;
        let isCountingDown = true;

        // カウントダウンを開始する
        let countdownInterval;
        let countupInterval;

        if (timeLeft != 3599000) { // 3599000 (00:59:59)
            if (timeLeft < 0) {
                timeLeft = -1 * timeLeft + 10 * 60000; // カウントダウンが終了するまで、10分を加算
                isCountingDown = false;
            }

            // 初期のtdコンテンツを更新する
            updateDisplay(element, timeLeft);

            if (timeLeft > 0 && isCountingDown) {
                countdownInterval = setInterval(function () {
                    if (timeLeft > 0)
                        timeLeft -= 1000;

                    updateDisplay(element, timeLeft);

                    // カウントダウンが終了したかどうかを確認する
                    if (timeLeft <= 0) {
                        clearInterval(countdownInterval);

                        // 初期の時間から再びカウントダウンを開始する
                        timeLeft = startTime;
                        isCountingDown = false;
                        timeLeft = 10 * 60000; // カウントダウンが終了するまで、10分を加算

                        countupInterval = setInterval(function () {
                            timeLeft += 1000;
                            element.className = 'timeCount redflag'

                            if (timeLeft == 3599000)
                                clearInterval(countupInterval);

                            updateDisplay(element, timeLeft);
                        }, 1000);
                    }
                }, 1000);
            } else {
                isCountingDown = false;
                countupInterval = setInterval(function () {
                    timeLeft += 1000;
                    element.className = 'timeCount redflag'

                    if (timeLeft == 3599000) 
                        clearInterval(countupInterval);

                    updateDisplay(element, timeLeft);
                }, 1000);
            }
        } else {
            countupInterval = setInterval(function () {
                element.className = 'timeCount redflag'
                updateDisplay(element, timeLeft);
            }, 10);
        }
    }

    // 時間を引く
    function subtractTime(today, resDateTime, subResult) {
        var differenceTime = Math.abs(today - resDateTime);
        // ミリ秒から時間、分、秒に変換する
        var minuteResult = Math.floor((differenceTime % (1000 * 60 * 60)) / (1000 * 60));
        var secondResult = Math.floor((differenceTime % (1000 * 60)) / 1000);

        // 結果を 'HH:mm:ss' 形式の文字列にフォーマットする
        subResult = `${(10 - minuteResult).toString().padStart(2, '0')}:${(0 - secondResult.toString().padStart(2, '0'))}`;
        return subResult;
    }

    // カウントダウン時に背景を変更する
    setInterval(function () {
        var tdElements = document.querySelectorAll('td.redflag');
        tdElements.forEach(function (tdElement) {
            var trElement = tdElement.parentElement;
            trElement.style.backgroundColor = '#FFC1C1';
        });
    }, 10);

    // tdのコンテンツを更新する関数
    function updateDisplay(element, time) {
        var minutes = Math.floor(time / 60000);
        if (minutes >= 60)
            minutes = 0;
        var seconds = Math.floor((time % 60000) / 1000);
        element.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

    // 各tdを繰り返し、各tdにカウントダウン関数を適用する
    const tdElements = document.querySelectorAll('#tblSupplyLeft td.timeCount');
    const tdElements1 = document.querySelectorAll('#tblSupplyRight td.timeCount');
    tdElements.forEach(countdown);
    tdElements1.forEach(countdown);

    // ローディング表示
    $(document).ajaxSend(function () {
        $("#overlay").fadeIn(300);
    });

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
                        url: 'Supply/Complete',
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







