
Authors: Anastasia Gonzalez, Conner Soule & Chrsitopher de Frietas
UID: u0985898, u1065896, & 
Date: Decmeber 06, 2019
Class: Web Software Architecture

GitHub: https://github.com/christopherdef/UFOU

Abstract:
	Convenient access to UFO sighting data is essential to better understand our universe and prepare 
	ourselves for life outside of our galaxy. This project scrapes UFO sighting data from the National 
	UFO Reporting Center (NUFORC) and compiles it into one database. Next, analysis of the data in the 
	database is used to find commonalities in patterns and sightings, decomposing thousands of reports
	into easily digestible statistics. Finally, visualization of the data is displayed to the user on
	a web page in simple graphs, charts, and maps providing users the information they need to be on the 
	lookout for the next UFO event.

Authorization:
	I followed the instructions shown in class to make user roles authorized as well as the following tutorials:
		https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-2.2&tabs=visual-studio
		https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-2.2&tabs=visual-studio
	For the role menus I used this tutorial's suggestion. (We spoke of a different way to do it in class but I didnt understand it): 
		https://www.codeproject.com/Articles/1237650/ASP-NET-Core-User-Role-Base-Dynamic-Menu-Managemen
	For passing multiple models to the view:
		https://www.c-sharpcorner.com/UploadFile/ff2f08/multiple-models-in-single-view-in-mvc/ 

Bootstrap & CSS components:
    • Navigation Bar: https://www.w3schools.com/bootstrap4/bootstrap_navbar.asp
    • Background Image: https://www.w3schools.com/html/html_images_background.asp

Outside Resources:
    The following was reference code to build the graphs and maps in the web browser
		https://plot.ly/javascript/
    UFO reports were scrapped from the following webpage:
		http://www.nuforc.org/webreports.html
