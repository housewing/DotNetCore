// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


var sTimeout = 5 * 60 * 1000;
setTimeout('SessionEnd()', sTimeout);

function SessionEnd() {
    window.location = "/Home/LoginMsg";
}