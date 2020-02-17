angular.module('umbraco').controller('SkybrudUmbracoRedirects.ImportDialog.Controller',
    function ($scope, notificationsService, Upload, editorService) {

        var vm = this;

        vm.invalidFileFormat = false;
        vm.rebuildInput = 1;
        vm.processing = false;
        vm.processed = false;
        vm.file = null;
        vm.success = false;
        vm.error = false;

        $scope.model.title = "Import redirects";

        vm.buttonState = 'init';

        vm.handleFiles = handleFiles;
        vm.upload = upload;
        vm.close = close;

        function upload(file) {
            if (vm.file === null) {
                vm.noFile = true;
                $timeout(function () {
                    vm.noFile = false;
                }, 500);
                return;
            }
            vm.processing = true;
            vm.buttonState = 'busy';
            Upload.upload({
                url: "backoffice/Skybrud/Redirects/Import",
                file: vm.file
            }).success(function (data, status, headers, config) {
                vm.success = "Redirects have been successfully uploaded."
                vm.error = false;
                vm.processing = false;
                vm.processed = true;
            }).error(function (data, status, headers, config) {
                if (data.Errors) {
                    vm.error = data.Errors;
                } else {
                    vm.error = "An error has occured";
                }
                vm.success = false;
                vm.processing = false;
                vm.processed = true;
            });
        }

        function handleFiles(files, event) {
            if (files && files.length > 0) {
                vm.file = files[0];
            }
        }

        function close() {
            vm.invalidFileFormat = false;
            vm.file = null;
            vm.fileName = null;
            vm.success = false;
            vm.error = false;
            vm.processing = false;
            vm.processed = false;
            vm.rebuildInput += 1;
            $('#file').val(null);

            if ($scope.model.close) {
                $scope.model.close();
            } else {
                editorService.close();
            }
        };

        $scope.$on("filesSelected", function (event, args) {
            if (args.files.length > 0) {
                vm.file = args.files[0];
                vm.fileName = vm.file.name;
            } else if (args.files.length <= 0 || vm.processing) {
                vm.file = null;
                return;
            }

            vm.noFile = false;

            var extension = vm.fileName.substring(vm.fileName.lastIndexOf(".") + 1, vm.fileName.length).toLowerCase();
            if (extension !== 'csv') {
                vm.invalidFileFormat = true;
                $timeout(function () {
                    vm.rebuildInput += 1;
                    vm.file = null;
                    vm.invalidFileFormat = false;
                }, 500);
                return;
            }
        });
    });