var ImgAI = function () {

    var demo = function () {
        function analyzeimg()
        {
            var fpath = $('#imgfolder').val();
            if (fpath == '')
            {
                alert('Image Folder should not be empty!');
                return false;
            }

            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Main/VCSELAIDemoData', {
                fpath: fpath
            }, function (output) {

                $.bootstrapLoading.end();

                $('.imgcontent').empty();
                $.each(output.urllist, function (i, val) {

                    var appendstr = '<div class="row form-group"><div class="col-xs-2"></div><div class="col-xs-8">';
                    appendstr += '<img src="'+val+'" />'
                    appendstr += '</div><div class="col-xs-2"></div></div>'
                    appendstr += '<hr>';

                    $(".imgcontent").append(appendstr);
                });

            });

        }

        $('body').on('click', '#btn-option', function () {
            analyzeimg();
        })
    }
    
    var demo2 = function () {
        function analyzeimg2() {
            var fpath = $('#file1').val();
            if (fpath == '') {
                alert('Image file should not be empty!');
                return false;
            }

            var options = {
                loadingTips: "Data Loading.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.ajaxFileUpload({
                url: '/Main/OCRServiceDemoData',
                secureuri: false,
                data: {},
                fileElementId: 'file1',
                dataType: 'json',
                success: function (data, status) {
                    $.bootstrapLoading.end();
                    if (data.sucess)
                    {
                        $('.imgcontent').empty();
                        var appendstr = '<div class="row form-group"><div class="col-xs-2"></div><div class="col-xs-8">';
                        appendstr += '<img src="' + data.url + '" />'
                        appendstr += '</div><div class="col-xs-2"></div></div>'
                        appendstr += '<hr>';
                        $(".imgcontent").append(appendstr);

                        $.each(data.charlist, function (i, val) {
                            var appendstr1 = '<div class="row form-group"><div class="col-xs-2"></div><div class="col-xs-4">';
                            appendstr1 += '<img src="' + val.img + '" />'
                            appendstr1 += '</div><div class="col-xs-4"><h2 style="text-align:center"><strong>' + val.cv
                            appendstr1 += '</strong></h2></div><div class="col-xs-2"></div></div>'
                            appendstr1 += '<hr>';
                            $(".imgcontent").append(appendstr1);
                        });
                    }
                },
                error: function (e) {
                    $.bootstrapLoading.end();
                    alert("something wrong.......");
                    console.log(e);
                }
            });

            //$.post('/Main/OCRServiceDemoData', {
            //    fpath: fpath
            //}, function (output) {

            //    $.bootstrapLoading.end();

            //    //$('.imgcontent').empty();
            //    //$.each(output.urllist, function (i, val) {

            //    //    var appendstr = '<div class="row form-group"><div class="col-xs-2"></div><div class="col-xs-8">';
            //    //    appendstr += '<img src="' + val + '" />'
            //    //    appendstr += '</div><div class="col-xs-2"></div></div>'
            //    //    appendstr += '<hr>';

            //    //    $(".imgcontent").append(appendstr);
            //    //});

            //});

        }

        $('body').on('click', '#btn-option', function () {
            analyzeimg2();
        })
    }

    return {
        DemoFun: function () {
            demo();
        },
        DemoFun2: function () {
            demo2();
        }
    }

}();