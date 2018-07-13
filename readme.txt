Чтобы проект Android работал в Release mode необходимо:
1. в параметрах Android пропустить компоновку сборок: mscorlib;System
2. в MainActivity: 
	HelloMemo.App myApp = new App(); 
	LoadApplication(myApp); 