(function () {



    angular.module(APP.NAME).controller('ContentManagementController', ContentManagementController);

    ContentManagementController.$inject = ["$stateParams", "$state", "$log", "$window", "$location", "cmsService", "toastr", "$http", '$uibModal', "$sce"];

    angular.module(APP.NAME).run(function (editableOptions) {
        editableOptions.theme = 'default';
    });



    function ContentManagementController($stateParams, $state, $log, $window, $location, cmsService, toastr, $http, $uibModal, $sce) {




        var vm = this;
        vm.$onInit = _onInit;
        vm.editContent = _editContent;
        vm.open = _open;
        vm.exists = false;
        vm.showCkEditor = _showCkeditor;
        vm.bindHtml = _bindHtml;
        vm.go = _go;


        function _onInit() {

            var pageId = $stateParams.id;

            cmsService.getContent(pageId, _contentSuccess, _contentError);

        }

        //------------------------------------------------- success/error -------------------------------------------------//

        function _contentSuccess(response) {
            toastr.success('Success...');
            $log.log(response);
            var response = response.data.items;
            vm.formContent = response;
        }

        function _contentError(response) {
            toastr.error(response.data, "Error...");
        }

        function _updateValuesSuccess(response) {
            toastr.success("Success...");
            vm.exists = false;
            
        }

        function _updateValuesError(response) {
            toastr.error(response.data, "Error...");
         
        }

        function _newContentSuccess(response) {
            toastr.success("Success");
            vm.exists = false;
        }

        function _newContentError(response) {
            toastr.error(response.data);
        }
        //-------------------------------------------------- handlers/events --------------------------------------------//

        function _editContent(content) {
        
            debugger
            if (content.id > 0) {

                cmsService.changeContent(content, _updateValuesSuccess, _updateValuesError);
                vm.ckeditorBody = $sce.trustAsHtml(content.value);

            }
            else {
                var object = {
                    "templateKeyId": content.keyId,
                    "Value": content.value,
                    "CMSPageId": content.pageId
                };
                cmsService.addNewContent(object, _newContentSuccess, _newContentError);
                vm.ckeditorBody = $sce.trustAsHtml(content.value);
            }
        }

        function _showCkeditor(content) {
            vm.exists = true;
            
        }

        function _open(size, currentField) {

            var modalInstance = $uibModal.open({
                controller: 'ImagePicker'
                , controllerAs: 'modalCtrl'
                , animation: true
                , ariaLabelledBy: 'modal-title'
                , ariaDescribedBy: 'modal-body'
                , templateUrl: '/app/admin/modules/CMS/views/imagePicker.html'
                , size: size
                , resolve: {
                    model: currentField
                }
            });



        };

        function _bindHtml(content) {
            if (content == null) {

                return $sce.trustAsHtml('<span style="color: red;">empty<span>');

            }

            return $sce.trustAsHtml(content);

        }

        function _go(url) {
            $window.open("/content/" + url, "_blank");
        }
        


    }

})();