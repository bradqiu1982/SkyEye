IMGTRAINING = function () {
    var ogpimgtraining = function () {
        var imgtable = null;

        $.fn.dataTable.ext.buttons.trainning = {
            text: 'Trainning',
            action: function (e, dt, node, config) {
                var kvarray = new Array();
                $('.valclass').each(function (i, val) {
                    var vkey = $(this).attr('vkey');
                    var vval = $(this).val();
                    if (vval != '') {
                        kvarray.push(vkey + ":::" + vval);
                    }
                });
                $.post('/OGPXY/UpdateTrainingData', {
                    imgkv: JSON.stringify(kvarray)
                }, function (output) {
                    alert('Image Training Sucessfully!');
                });
            }
        };

        function solveimgdata(output, traintype)
        {
            if (imgtable) {
                imgtable.destroy();
                imgtable = null;
            }

            $("#imghead").empty();
            $("#imgcontent").empty();

            if (traintype == 'OPTTRAINING') {
                $("#imghead").append(
                    '<tr>' +
                    '<th>Capture Img</th>' +
                    //'<th>Child Index</th>' +
                    '<th>Raw Img</th>' +
                    '<th>Child Img</th>' +
                    '<th>Reference Value</th>' +
                    '<th>Training Value</th>' +
                    '</tr>'
                );
                $.each(output.imglist, function (i, val) {
                    var capimg = '<img src="data:image/png;base64,' + val.capimg + '" />';
                    var rawimg = '<a href="' + val.rawurl + '" target="_blank">RAWImg</a>';
                    var chimg = '<img src="data:image/png;base64,' + val.chimg + '" />';
                    //var imgval = '<input type="text" class="valclass" vkey="' + val.cimgkey + '" value="' + val.cimgval + '" />';
                    var imgval = '<input type="text" class="valclass" vkey="' + val.cimgkey + '" value="" />';

                    $("#imgcontent").append(
                        '<tr class="CFD' + val.cfdlevel + '">' +
                        '<td>' + capimg + '</td>' +
                        //'<td>' + val.chidx + '</td>' +
                        '<td>' + rawimg + '</td>' +
                        '<td>' + chimg + '</td>' +
                        '<td>' + val.cimgval + '</td>' +
                        '<td>' + imgval + '</td>' +
                        '</tr>'
                    );
                });
            }
            else {
                $("#imghead").append(
                        '<tr>' +
                        '<th>Capture Img</th>' +
                        '<th>Child Index</th>' +
                        '<th>XCoord</th>' +
                        '<th>YCoord</th>' +
                        '<th>Raw Img</th>' +
                        '<th>Child Img</th>' +
                        '<th>Value</th>' +
                        '</tr>'
                    );
                $.each(output.imglist, function (i, val) {
                    var capimg = '<img src="data:image/png;base64,' + val.capimg + '" />';
                    var rawimg = '<a href="' + val.rawurl + '" target="_blank">RAWImg</a>';
                    var chimg = '<img src="data:image/png;base64,' + val.chimg + '" />';
                    var imgval = '<input type="text" class="valclass" vkey="' + val.cimgkey + '" value="' + val.cimgval + '" />';


                    $("#imgcontent").append(
                        '<tr>' +
                        '<td>' + capimg + '</td>' +
                        '<td>' + val.chidx + '</td>' +
                        '<td>' + val.xcoord + '</td>' +
                        '<td>' + val.ycoord + '</td>' +
                        '<td>' + rawimg + '</td>' +
                        '<td>' + chimg + '</td>' +
                        '<td>' + imgval + '</td>' +
                        '</tr>'
                    );
                });
            }

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
                buttons: ['trainning']
            });
        }

        function loadnewtrainingimgs(traintype) {
            var wafernum = $('#wafernum').val().trim();
            var fpath = $('#imgfolder').val().trim();

            if (fpath == '' || wafernum == '')
            { alert('Please input the share folder path of the images and the wafer number!'); return false; }

            if (wafernum.indexOf('E') == -1
                && wafernum.indexOf('R') == -1
                && wafernum.indexOf('T') == -1)
            { alert('wafer number should contains E or R or T!'); return false;}

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/OGPXY/NewImgTrain', {
                fpath: fpath,
                wafer: wafernum
            }, function (output) {
                $.bootstrapLoading.end();
                solveimgdata(output, traintype)
                if (output.failimg != '')
                { alert("FAIL TO ANALYZE FOLLOWING FILES:" + output.failimg); }
                if (output.MSG != '')
                { alert(output.MSG); }
            });
        }

        function loadexisttrainingimgs(traintype) {
            var wafernum = $('#wafernum').val().trim();
            if (wafernum == '')
            { alert('Please input the wafer number!'); return false; }

            //if (wafernum.indexOf('E') == -1
            //    && wafernum.indexOf('R') == -1
            //    && wafernum.indexOf('T') == -1)
            //{ alert('wafer number should contains E or R or T!'); return false; }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/OGPXY/ExistImgTrain', {
                wafer: wafernum
            }, function (output) {
                $.bootstrapLoading.end();
                solveimgdata(output, traintype)
            });
        }

        $('body').on('click', '#btn-all', function () {
            var fpath = $('#imgfolder').val().trim();
            var wafernum = $('#wafernum').val().trim();
            if (fpath != '' && wafernum != '') {
                loadnewtrainingimgs('ALLTRAINING');
            }
            else if (wafernum != '') {
                loadexisttrainingimgs('ALLTRAINING');
            }
            
        });

        $('body').on('click', '#btn-option', function () {
            var fpath = $('#imgfolder').val().trim();
            var wafernum = $('#wafernum').val().trim();
            if (fpath != '' && wafernum != '') {
                loadnewtrainingimgs('OPTTRAINING');
            }
            else if (wafernum != ''){
                loadexisttrainingimgs('OPTTRAINING');
            }
        });
    }

    var ogpxycompare = function () {
        var imgtable = null;

        $.fn.dataTable.ext.buttons.updatexy = {
            text: 'Update XY',
            action: function (e, dt, node, config) {
                var kvarray = new Array();
                $('.valclass').each(function (i, val) {
                    var vkey = $(this).attr('vkey');
                    var vval = $(this).val();
                    if (vval != '') {
                        kvarray.push(vkey + ":::" + vval);
                    }
                });
                $.post('/OGPXY/UpdateOGPXYData', {
                    imgkv: JSON.stringify(kvarray)
                }, function (output) {
                    alert('XY Update Sucessfully!');
                    comparingogpxy();
                });
            }
        };

        function solverecognizeresult(output)
        {
            if (imgtable) {
                imgtable.destroy();
                imgtable = null;
            }

            $("#imghead").empty();
            $("#imgcontent").empty();

            $("#imghead").append(
                '<tr>' +
                '<th>SN</th>' +
                '<th>RAW Img</th>' +
                '<th>Capture Img</th>' +
                '<th>NPI-X</th>' +
                '<th>NEW-X</th>' +
                '<th>NPI-Y</th>' +
                '<th>NEW-Y</th>' +
                //'<th>ME-X</th>' +
                //'<th>ME-Y</th>' +
                '</tr>'
            );

            $.each(output.xylist, function (i, val) {
                var rawimg = '<a href="' + val.RawImg + '" target="_blank">RAWImg</a>';
                var capimg = '<img src="data:image/png;base64,' + val.CaptureImg + '" />';
                var imgxval = '<input type="text" class="valclass" vkey="' + val.MainImgKey + ':::X" value="" />';
                var imgyval = '<input type="text" class="valclass" vkey="' + val.MainImgKey + ':::Y" value="" />';

                $("#imgcontent").append(
                    '<tr class="CFD' + val.CFDLevel + '">' +
                    '<td>' + val.SN + '</td>' +
                    '<td>' + rawimg + '</td>' +
                    '<td>' + capimg + '</td>' +
                    '<td>' + val.X + '</td>' +
                    '<td>' + imgxval + '</td>' +
                    '<td>' + val.Y + '</td>' +
                    '<td>' + imgyval + '</td>' +
                    //'<td>' + val.MX + '</td>' +
                    //'<td>' + val.MY + '</td>' +
                    '</tr>'
                );
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
                buttons: ['updatexy', 'excelHtml5']
            });
        }

        function comparingogpxy() {
            var wafernum = $('#wafernum').val().trim();
            if (wafernum == '')
            { alert('Please input the wafer number!'); return false; }

            //if (wafernum.indexOf('E') == -1
            //    && wafernum.indexOf('R') == -1
            //    && wafernum.indexOf('T') == -1)
            //{ alert('wafer number should contains E or R or T!'); return false; }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/OGPXY/OGPXYCompareData', {
                wafernum: wafernum
            }, function (output) {

                $.bootstrapLoading.end();
                solverecognizeresult(output);
            });
        }

        function recognizeogpxy() {
            var wafernum = $('#wafernum').val().trim();
            var fpath = $('#imgfolder').val().trim();
            var alg = $('#alg').val();
            var vtype = $('#vtype').val();

            var fixangle = 'FALSE';
            var newalg = 'FALSE';

            if (alg == 'gen')
            { }
            else if (alg == 'genangle')
            { fixangle = 'TRUE'; }
            else if (alg == 'newalg')
            { newalg = 'TRUE'; }
            else if (alg == 'newalgangle')
            { fixangle = 'TRUE'; newalg = 'TRUE'; }

            if (fpath == '' || wafernum == '')
            { alert('Please input the share folder path of the images and the wafer number!'); return false; }

            if (wafernum.indexOf('E') == -1
                && wafernum.indexOf('R') == -1
                && wafernum.indexOf('T') == -1)
            { alert('wafer number should contains E or R or T!'); return false; }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/OGPXY/OGPXYRecognize', {
                fpath: fpath,
                wafer: wafernum,
                fixangle: fixangle,
                newalg: newalg,
                vtype: vtype
            }, function (output) {
                $.bootstrapLoading.end();
                solverecognizeresult(output);
                if (output.MSG != '')
                { alert(output.MSG); }
            });
        }

        $('body').on('click', '#btn-recognize', function () {
            $('#btn-recognize').attr('disabled', true);
            recognizeogpxy();
            $('#btn-recognize').removeAttr('disabled');
        });

        //$('body').on('click', '#btn-recognangle', function () {
        //    $('#btn-recognangle').attr('disabled', true);
        //    recognizeogpxy("TRUE");
        //    $('#btn-recognangle').removeAttr('disabled');
        //});

        $('body').on('click', '#btn-review', function () {
            comparingogpxy();
        });
    }

    var ogpprofreviewfun = function () {
        var imgtable = null;

        //$.fn.dataTable.ext.buttons.trainning = {
        //    text: 'Trainning',
        //    action: function (e, dt, node, config) {
        //        var kvarray = new Array();
        //        $('.valclass').each(function (i, val) {
        //            var vkey = $(this).attr('vkey');
        //            var vval = $(this).val();
        //            if (vval != '') {
        //                kvarray.push(vkey + ":::" + vval);
        //            }
        //        });
        //        $.post('/OGPXY/UpdateTrainingData', {
        //            imgkv: JSON.stringify(kvarray)
        //        }, function (output) {
        //            alert('Image Training Sucessfully!');
        //        });
        //    }
        //};

        function solveimgdata(output, traintype) {
            if (imgtable) {
                imgtable.destroy();
                imgtable = null;
            }

            $("#imghead").empty();
            $("#imgcontent").empty();

            if (traintype == 'OPTTRAINING') {
                $("#imghead").append(
                    '<tr>' +
                    '<th>Capture Img</th>' +
                    '<th>Child Index</th>' +
                    '<th>Raw Img</th>' +
                    '<th>Child Img</th>' +
                    '<th>Reference Value</th>' +
                    //'<th>Training Value</th>' +
                    '</tr>'
                );
                $.each(output.imglist, function (i, val) {
                    var capimg = '<img src="data:image/png;base64,' + val.capimg + '" />';
                    var rawimg = '<a href="' + val.rawurl + '" target="_blank">RAWImg</a>';
                    var chimg = '<img src="data:image/png;base64,' + val.chimg + '" />';
                    //var imgval = '<input type="text" class="valclass" vkey="' + val.cimgkey + '" value="' + val.cimgval + '" />';
                    var imgval = '<input type="text" class="valclass" vkey="' + val.cimgkey + '" value="" />';

                    $("#imgcontent").append(
                        '<tr class="CFD' + val.cfdlevel + '">' +
                        '<td>' + capimg + '</td>' +
                        '<td>' + val.chidx + '</td>' +
                        '<td>' + rawimg + '</td>' +
                        '<td>' + chimg + '</td>' +
                        '<td>' + val.cimgval + '</td>' +
                        //'<td>' + imgval + '</td>' +
                        '</tr>'
                    );
                });
            }
            else {
                $("#imghead").append(
                        '<tr>' +
                        '<th>Capture Img</th>' +
                        '<th>Child Index</th>' +
                        '<th>XCoord</th>' +
                        '<th>YCoord</th>' +
                        '<th>Raw Img</th>' +
                        '<th>Child Img</th>' +
                        '<th>Value</th>' +
                        '</tr>'
                    );
                $.each(output.imglist, function (i, val) {
                    var capimg = '<img src="data:image/png;base64,' + val.capimg + '" />';
                    var rawimg = '<a href="' + val.rawurl + '" target="_blank">RAWImg</a>';
                    var chimg = '<img src="data:image/png;base64,' + val.chimg + '" />';
                    var imgval = '<input type="text" class="valclass" vkey="' + val.cimgkey + '" value="' + val.cimgval + '" />';

                    $("#imgcontent").append(
                        '<tr>' +
                        '<td>' + capimg + '</td>' +
                        '<td>' + val.chidx + '</td>' +
                        '<td>' + val.xcoord + '</td>' +
                        '<td>' + val.ycoord + '</td>' +
                        '<td>' + rawimg + '</td>' +
                        '<td>' + chimg + '</td>' +
                        '<td>' + imgval + '</td>' +
                        '</tr>'
                    );
                });
            }

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
        }

        function loadexisttrainingimgs(traintype) {
            var wafernum = $('#wafernum').val().trim();
            if (wafernum == '')
            { alert('Please input the wafer number!'); return false; }

            //if (wafernum.indexOf('E') == -1
            //    && wafernum.indexOf('R') == -1
            //    && wafernum.indexOf('T') == -1)
            //{ alert('wafer number should contains E or R or T!'); return false; }

            var options = {
                loadingTips: "loading data......",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/OGPXY/ExistImgTrain', {
                wafer: wafernum
            }, function (output) {
                $.bootstrapLoading.end();
                solveimgdata(output, traintype)
            });
        }

        $('body').on('click', '#btn-option', function () {
            var wafernum = $('#wafernum').val().trim();
            if (wafernum != '') {
                loadexisttrainingimgs('OPTTRAINING');
            }
        });
    }

    return {
        OGPIMGINIT: function () {
            ogpimgtraining();
        },
        OGPXYCMP: function () {
            ogpxycompare();
        },
        OGPPROFREVIEW: function ()
        {
            ogpprofreviewfun();
        }
    }
}();