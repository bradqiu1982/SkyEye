IMGTRAINING = function () {
    var newimgtraining = function ()
    {
        var imgtable = null;

        $.fn.dataTable.ext.buttons.trainning = {
            text: 'Trainning',
            action: function (e, dt, node, config) {
                var kvarray = new Array();
                $('.valclass').each(function (i, val) {
                    var vkey = $(this).attr('vkey');
                    var vval = $(this).val();
                    if (vval != '')
                    {
                        kvarray.push(vkey + ":::" + vval);
                    }
                });
                $.post('/main/UpdateTrainingData', {
                    imgkv:JSON.stringify(kvarray)
                }, function (output) {
                    alert('Image Training Sucessfully!');
                });
            }
        };

        function loadimgs(cond,url)
        {
            $.post(url, {
                cond:cond
            }, function (output) {

                if (imgtable) {
                    imgtable.destroy();
                    imgtable = null;
                }

                $("#imghead").empty();
                $("#imgcontent").empty();

                $("#imghead").append(
                        '<tr>' +
                        '<th>Capture Img</th>' +
                        '<th>Raw Img</th>' +
                        '<th>Child Index</th>' +
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
                        '<td>' + rawimg + '</td>' +
                        '<td>' + val.chidx + '</td>' +
                        '<td>' + chimg + '</td>' +
                        '<td>' + imgval + '</td>' +
                        '</tr>'
                    );
                });



                imgtable = $('#imgtable').DataTable({
                    'iDisplayLength': 20,
                    'aLengthMenu': [[20, 50, 100, -1],
                    [20, 50, 100, "All"]],
                    "columnDefs": [
                        { "className": "dt-center", "targets": "_all" }
                    ],
                    "aaSorting": [],
                    "order": [],
                    dom: 'lBfrtip',
                    buttons: ['trainning']
                });
            });
        }

        $('body').on('click', '#btn-newgo', function () {
            var cond = $('#imgfolder').val();
            var url = '/main/NewImgTrain';
            if (cond == '')
            { alert('if you want to training new img,please input the share folder path of the images!'); return false; }

            loadimgs(cond,url);
        });

        $('body').on('click', '#btn-existgo', function () {
            var cond = $('#caprev').val();
            var url = '/main/ExistImgTrain';
            loadimgs(cond, url);
        });
    }

    return {
        NEWIMGINIT: function () {
            newimgtraining();
        }
    }
}();