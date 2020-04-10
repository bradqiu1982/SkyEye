OCR = function () {
    var ocropttools = function () {
        var imgtable = null;

        $('.date').datepicker({ autoclose: true, viewMode: "days", minViewMode: "days" });

        $.post('/OGPXY/GetOCRLotnumList', {
        }, function (output) {
            $('#lotnum').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.vallist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#lotnum').attr('readonly', false);
        });

        $.post('/OGPXY/GetOCRProdList', {
        }, function (output) {
            $('#prod').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.vallist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#prod').attr('readonly', false);
        });

        $.post('/OGPXY/GetOCRMacList', {
        }, function (output) {
            $('#mac').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.vallist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#mac').attr('readonly', false);
        });

        var getocrvm = function () {
            var lotnum = $('#lotnum').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            var prod = $('#prod').val();
            var mac = $('#mac').val();

            var passcheck = false;
            if (lotnum != '') { passcheck = true; }
            if (prod != '' && sdate != '' && edate != '') { passcheck = true; }
            if (mac != '' && sdate != '' && edate != '') { passcheck = true; }

            if (!passcheck)
            { alert('Please input your query condition'); return false; }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/OGPXY/GetOCRVM', {
                lotnum: lotnum,
                sdate: sdate,
                edate: edate,
                prod: prod,
                mac: mac
            }, function (output) {
                $('#ocrkey').val(output.ocrkey);
                $('#conf').val(output.uploader);

                if (imgtable) {
                    imgtable.destroy();
                    imgtable = null;
                }

                $("#imghead").empty();
                $("#imgcontent").empty();

                $("#imghead").append(
                '<tr>' +
                '<th>Lot Num</th>' +
                '<th>Product</th>' +
                '<th>SN</th>' +
                '<th>RAW Img</th>' +
                '<th>Capture Img</th>' +
                '<th>NPI-X</th>' +
                '<th>NEW-X</th>' +
                '<th>NPI-Y</th>' +
                '<th>NEW-Y</th>' +
                '</tr>'
                );

                $.each(output.ocrlist, function (i, ocr) {
                    $.each(ocr.XYList, function (i, val) {
                        var rawimg = '<a href="' + val.RawImg + '" target="_blank">RAWImg</a>';
                        var capimg = '<img src="data:image/png;base64,' + val.CaptureImg + '" />';
                        var imgxval = '<input type="text" class="valclass" vkey="' + val.MainImgKey + ':::X" value="" />';
                        var imgyval = '<input type="text" class="valclass" vkey="' + val.MainImgKey + ':::Y" value="" />';

                        $("#imgcontent").append(
                            '<tr>' +
                            '<td>' + ocr.LotNum + '</td>' +
                            '<td>' + ocr.Product + '</td>' +
                            '<td>' + val.SN + '</td>' +
                            '<td>' + rawimg + '</td>' +
                            '<td>' + capimg + '</td>' +
                            '<td>' + val.X + '</td>' +
                            '<td>' + imgxval + '</td>' +
                            '<td>' + val.Y + '</td>' +
                            '<td>' + imgyval + '</td>' +
                            '</tr>'
                        );
                    });
                });

                imgtable = $('#imgtable').DataTable({
                    'iDisplayLength': -1,
                    'aLengthMenu': [[-1],
                    ["All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['excelHtml5']
                });

                $.bootstrapLoading.end();
            });

        }

        $('body').on('click', '#btn-query', function () {
            getocrvm();
        });

        $('body').on('click', '#btn-query1', function () {
            getocrvm();
        });

        $('body').on('click', '#btn-conf', function () {
            var ocrkey = $('#ocrkey').val();
            if (ocrkey == '') { return false; }

            var conf = $('#conf').val();
            if (conf == '')
            { alert('Please input your name.'); return false; }
            
            var kvarray = new Array();
            $('.valclass').each(function (i, val) {
                var vkey = $(this).attr('vkey');
                var vval = $(this).val();
                if (vval != '') {
                    kvarray.push(vkey + ":::" + vval);
                }
            });

            var xyval = JSON.stringify(kvarray);
            $.post('/OGPXY/ConfirmOCRInfo', {
                ocrkey: ocrkey,
                conf: conf,
                xyval: xyval
            }, function (output) {
                alert('XY Information Confirm Sucessfully!');
                getocrvm();
            });

        });

    }


    var ocrauditfun = function () {
        var imgtable = null;

        $('.date').datepicker({ autoclose: true, viewMode: "days", minViewMode: "days" });

        $.post('/OGPXY/GetOCRLotnumList', {
        }, function (output) {
            $('#lotnum').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.vallist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#lotnum').attr('readonly', false);
        });

        $.post('/OGPXY/GetOCRProdList', {
        }, function (output) {
            $('#prod').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.vallist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#prod').attr('readonly', false);
        });

        $.post('/OGPXY/GetOCRMacList', {
        }, function (output) {
            $('#mac').autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    term = term.toLowerCase();
                    var choices = output.vallist;
                    var suggestions = [];
                    for (i = 0; i < choices.length; i++)
                        if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                    suggest(suggestions);
                }
            });
            $('#mac').attr('readonly', false);
        });

        var getocrvm1 = function () {
            var lotnum = $('#lotnum').val();
            var sdate = $('#sdate').val();
            var edate = $('#edate').val();
            var prod = $('#prod').val();
            var mac = $('#mac').val();

            var passcheck = false;
            if (lotnum != '') { passcheck = true; }
            if (prod != '' && sdate != '' && edate != '') { passcheck = true; }
            if (mac != '' && sdate != '' && edate != '') { passcheck = true; }

            if (!passcheck)
            { alert('Please input your query condition'); return false; }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/OGPXY/GetOCRAuditVM', {
                lotnum: lotnum,
                sdate: sdate,
                edate: edate,
                prod: prod,
                mac: mac
            }, function (output) {

                if (imgtable) {
                    imgtable.destroy();
                    imgtable = null;
                }

                $("#imghead").empty();
                $("#imgcontent").empty();

                $("#imghead").append(
                '<tr>' +
                '<th>Lot Num</th>' +
                '<th>Product</th>' +
                '<th>SN</th>' +
                '<th>RAW Img</th>' +
                '<th>Capture Img</th>' +
                '<th>NPI-X</th>' +
                '<th>NPI-Y</th>' +
                '<th>CF</th>' +
                '<th>CF-Time</th>' +
                '<th>Yield</th>' +
                '</tr>'
                );

                $.each(output.ocrlist, function (i, ocr) {
                    $.each(ocr.XYList, function (i, val) {
                        var rawimg = '<a href="' + val.RawImg + '" target="_blank">RAWImg</a>';
                        var capimg = '<img src="data:image/png;base64,' + val.CaptureImg + '" />';

                        $("#imgcontent").append(
                            '<tr class="' + val.Modified + '">' +
                            '<td>' + ocr.LotNum + '</td>' +
                            '<td>' + ocr.Product + '</td>' +
                            '<td>' + val.SN + '</td>' +
                            '<td>' + rawimg + '</td>' +
                            '<td>' + capimg + '</td>' +
                            '<td>' + val.X + '</td>' +
                            '<td>' + val.Y + '</td>' +
                            '<td>' + ocr.Confirmer + '</td>' +
                            '<td>' + ocr.ConfirmTime + '</td>' +
                            '<td>' + ocr.Yield + '</td>' +
                            '</tr>'
                        );
                    });
                });

                imgtable = $('#imgtable').DataTable({
                    'iDisplayLength': -1,
                    'aLengthMenu': [[-1],
                    ["All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['excelHtml5']
                });

                $.bootstrapLoading.end();
            });

        }

        $('body').on('click', '#btn-query', function () {
            getocrvm1();
        });

        $('body').on('click', '#btn-query1', function () {
            getocrvm1();
        });

    }

    return {
        OCROPTINIT: function () {
            ocropttools();
        },
        OCRAUDIT: function ()
        {
            ocrauditfun();
        }
    }
}();
