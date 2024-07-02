// -----------------------------------準備画面-----------------------------------//
    // url取得
    var baseUrl = window.location.origin;
    var pathName = window.location.pathname.split('/');
    if (pathName.length > 2)
        baseUrl = baseUrl + "/" + pathName[1];

    // SignalRを使用して接続を初期化する
    var connectionSupply = new signalR.HubConnectionBuilder().withUrl("preparationHub").build();
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
        connectionSupply.invoke("SendPreparations").catch(function (error) {
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

                        // 現在日時
                        var today = new Date();
                        // 依頼時間
                        var resDateTime = new Date(supplys[i].correctedRequestDatetime);

                        // 2つの日付のミリ秒単位の差を計算
                        var differenceTime = Math.abs(today - resDateTime);

                        // ミリ秒から時間、分変換
                        var minuteNum = Math.floor(differenceTime / (1000 * 60));

                        // mm:ss表記に変換
                        var subResult = "";
                        // 60:00以上はデフォルト"59:59"
                        if (minuteNum >= 60)
                            subResult = "59:59";
                        else
                            subResult = subtractTime(today, resDateTime, subResult);　// 時間を引く

                        var timeTmp = subResult.split(':');
                        var dataTime = ((parseInt(timeTmp[0]) * 60) + (parseInt(timeTmp[1]))) * 1000;

                        cell2.innerHTML = `${supplys[i].machineNum}`;
                        cell2.className = 'machine-number';
                        cell3.innerHTML = `${supplys[i].boxType}`;
                        cell3.className = 'boxType';

                        // 異なるテキストの長さに応じて文字サイズを調整
                        countLengthText(cell3);

                        cell4.innerHTML = `${supplys[i].boxCount}`;
                        cell4.className = 'boxCount';
                        cell5.innerHTML = `${subResult}`;
                        cell6.innerHTML = `<button type="button" class="btn btn-primary btnCompletion">完了</button>`;

                        if (dataTime < 0) {
                            cell5.className = 'timeCount redflag';
                        }
                        else
                            cell5.className = 'timeCount';

                        cell5.setAttribute("data-time", dataTime);
                        cell7.innerHTML = `${supplys[i].emptyBoxSupplyRequestId}`;
                        cell7.className = 'supplyId';
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

                    Swal.fire({
                        title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>準備完了登録を行います。<br>よろしいですか？`,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#0d6efd',
                        cancelButtonText: 'キャンセル',
                        confirmButtonText: '完了',
                        allowOutsideClick: false,
                    }).then((result) => {
                        if (result.isConfirmed) {
                            $.ajax({
                                type: 'POST',
                                url: baseUrl +'/Preparation/Complete',
                                data: { dataSupplyId: dataSupplyId },
                                success: function (response) {
                                    if (response.res != true) {
                                        setTimeout(function () {
                                            Swal.fire({
                                                icon: 'error',
                                                title: `箱種 ${dataBoxType} 箱数 ${dataBoxCount}の<br>準備完了登録ができませんでした。<br>再度お試しください。`,
                                                html: response.res,
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
    // ----------------------------------------------------------------------//


    // -----------------------------------運搬画面-----------------------------------//
    // SignalRを使用して接続を初期化する
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

                        // 2つの日付のミリ秒単位の差を計算
                        var differenceTime = Math.abs(today - resDateTime);

                        // ミリ秒から時間、分変換
                        var minuteNum = Math.floor(differenceTime / (1000 * 60));

                        // mm:ss表記に変換
                        var subResult = "";
                        // 60:00以上はデフォルト"59:59"
                        if (minuteNum >= 60)
                            subResult = "59:59";
                        else
                            subResult = subtractTime(today, resDateTime, subResult);　// 時間を引く

                        var timeTmp = subResult.split(':');
                        var dataTime = ((parseInt(timeTmp[0]) * 60) + (parseInt(timeTmp[1]))) * 1000;

                        cell2.innerHTML = `${transports[i].machineNum}`;
                        cell3.innerHTML = `${transports[i].boxType}`;
                        cell3.className = 'boxType';

                        // 異なるテキストの長さに応じて文字サイズを調整
                        countLengthText(cell3);

                        cell4.innerHTML = `${transports[i].boxCount}`;
                        cell4.className = 'boxCount';
                        cell5.innerHTML = `${subResult}`;
                        cell6.className = 'statusBtn';

                        if (transports[i].emptyBoxSupplyStatusId == 2)
                            cell6.innerHTML = `<button type="button" class="btn btn-warning btnRegister">開始</button>`;

                        if (transports[i].emptyBoxSupplyStatusId == 3)
                            cell6.innerHTML = `<button type="button" class="btn btn-success btnRegister btnEnd">終了</button>`;

                        if (dataTime < 0) {
                            cell5.className = 'timeCount redflag';
                        }
                        else
                            cell5.className = 'timeCount';

                        cell5.setAttribute("data-time", dataTime);
                        cell7.innerHTML = `${transports[i].emptyBoxSupplyRequestId}`;
                        cell7.className = 'transportId';
                        cell8.innerHTML = `<button type="button" class="btn btn-secondary btnClose"><i class="fa-solid fa-xmark"></i></button>`;
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
                                            html: response.res,
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
                                                    html: response.res,
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
                                            html: response.res,
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
                                            html: response.res,
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
         if (fullwidthCount  == 9) 
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
        $("#overlay").fadeIn();
    });

    // 時間表示(mm:ss)を更新
    function updateDisplay(element, time) {
        var minutes = Math.floor(time / 60000);
        if (minutes >= 60)
            minutes = 0;
        var seconds = Math.floor((time % 60000) / 1000);
        element.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

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