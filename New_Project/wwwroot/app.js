var app = angular.module("myApp", ["ngRoute"]);

app.config(function ($routeProvider, $locationProvider) {
    $locationProvider.hashPrefix(''); // add configuration
    $routeProvider
        .when("/home", {
            templateUrl: "/cap/Components/home/home.html",
            controller: "homeCtrl"
        })
        .otherwise({ redirectTo: '/home' });
});