# LambdaRoad - Command line application and tool backend

This is the repo for the LambdaRoad command line application, its unit tests, and a simple REST API which serves as a backend for the [web-based map tool](http://mobilitet.sintef.no/lambda/).

This readme is a very technical description of how to use the command line application. For a more visual documentation of the capabilities of the application, see the readme in the [tool repository](https://github.com/erlenddahl/lambdaroad-interface).

## All LambdaRoad repositories:
 - [Command line application and tool backend](https://github.com/erlenddahl/lambdaroad-model)
 - [Web-based map tool](https://github.com/erlenddahl/lambdaroad-interface)
 - [Tile server](https://github.com/erlenddahl/lambdaroad-tileserver)

# The command line application
The command line application is a .NET Standard application written in C#. It consists of one solution containing four projects:

 - LambdaModel is a library containing most of the code.
 - LambdaModel.Tests is a set of unit tests for various parts of the LambdaModel code.
 - LambdaModelRunner is the command line application utilizing the LambdaModel library to run calculations described in a JSON configuration file.
 - LambdaRestApi is the REST API that serves as a backend for the web-based map tool.

There are currently two implemented path loss models (mobile network 800MHz and ITS G5), as well as a framework for running path loss calculations on a road network, in a grid, or to a single point. It also includes methods for preprocessing elevation data from Hoydedata.no (or other equivalent sources) and converting it to a custom binary format that is quicker to read, ensuring speedy calculations.

The application is run by dragging a config file onto its file icon in Windows Explorer (LambdaModelRunner.exe), or by running LambdaModelRunner.exe 'path\to\some\config.json'.

If you want to use the command line application, you can either download this code and compile it yourself, or download the compiled version from the [release section](https://github.com/erlenddahl/lambdaroad-model/releases).

## Calculation modes
The application can handle two different types of config files, either for running a road network calculation, or for running an elevation data pre-processing. In addition, the application can run grid calculations and single point calculations, but these have not been the focus of the LambdaRoad project, and there has not been implemented a config entry point for them. A user competent in C# would be able to run them anyway, with some minor modifications to the application.

### Road network
A road network calculation is a calculation where the received signal strength from one or more base stations is calculated along all roads within the base station's range.

Road network calculations are described in detail in the section 'Running road network calculations' in the readme in the [tool repository](https://github.com/erlenddahl/lambdaroad-interface).

The JSON below shows an example of a config file for a road network calculation:

```javascript
{
    // Which type of operation this log file runs. Can be either RoadNetwork, Grid or GenerateTiles.
    "Operation": "RoadNetwork",              

    // A shape-file that contains the road network. Can be absolute or relative to the config location.
    // The road network can be downloaded from here: https://kartkatalog.geonorge.no/metadata/nvdb-ruteplan-nettverksdatasett/8d0f9066-34f9-4423-be12-8e8523089313
    "RoadShapeLocation": "2021-05-28.shp",   

    // How many meters between each point on a road link that is calculated. 
    // The lowest possible value is 1. A higher value will result in a more detailed result, but longer 
    // calculation times. 
    // The first and last point on the road link is guaranteed to be calculated even if this value is 
    // larger than the length of the link.
    "LinkCalculationPointFrequency": 10,
                                             

    // The regression formula used in path loss calculations for the mobile network has three different 
    // sets of constants. Possible values are:                                             
    //     'All' is trained on all values in the test set, 
    //     'LineOfSight' is trained on values with line of sight, 
    //     'NoLineOfSight' is trained on values without line of sight. 
    //     'Dynamic' will automatically pick either LOS or NLOS constants depending on whether the current 
    //         path loss calculation has line of sight or not.
    "MobileRegression": "Dynamic",
    
    // The height of the receiver above the terrain (meters).
    "ReceiverHeightAboveTerrain": 2,      

    // The minimum accepted RSRP. Any road links with a guaranteed lower RSRP than this (calculated 
    // using horizontal and vertical distances) will be excluded from the calculation. Any road links
    // with a lower RSRP than this will be excluded from the results. 
    // A higher value will result in much faster calculations and a smaller result file size.
    "MinimumAllowableRsrp": -115,            
                                             

    // How many threads that can be used to run calculation. 
    // If set to null, the number of threads will automatically be determined based on the number of 
    // processing cores. N threads means N times as much memory usage, so if you experience memory problems,
    // try setting this to a low number. There will never be more threads than base stations(one thread
    // processes one base station at a time).
    "CalculationThreads": null,              
                                             
    // A list (array, []) of all base stations to be included in this calculation.
    "BaseStations": [                        
        {
            // A unique ID for this base station. Must be unique across this configuration file.
            "id": "1254",                    
            // A name for this base station. Does not have to be unique. Only used in progress presentation.
            "name": "Mob1",                  
            
            "center": {
                // X-coordinate in UTM 33N (EPSG 25833, https://epsg.io/map#srs=25833)
                "x": 268283.4066450454,      
                // Y-coordinate in UTM 33N
                "y": 7040847.668103122       
            },
            
            // How many meters above the terrain this antenna is placed.
            "heightAboveTerrain": 12,        

            // The radius around this station that it is necessary for the road link calculation to consider 
            // road links. A higher radius prolongs the calculation time, as more road links must be considered.
            "maxRadius": 10000,              

            // The type of antenna. Can be either MobileNetwork for mobile network signals (800MHz) or ItsG5 
            // for ITS G5.
            "antennaType": "MobileNetwork",  

            // The (directional) gain of this station. 
            // It can be a single number for constant gain in all directions, or a sector definition for 
            // different gain in different directions. 
            // Constant example: '10' (10 Db all around the station)
            // Angle dependent example: '-45:45:18|45:60:7' (18 Db between -45 and 45 degrees, and 7 Db between
            // 45 and 60 degrees).
            // Default value if undefined: 0 (all around the station)
            "gainDefinition": "125:140:5|90:125:25|65:90:15|40:65:4", 
            
            // The base transmit power of this station. It is equal in all directions.
            // For RSRP calculations, power and (directional) gain is added before subtracting the different losses.
            "power": 46,                     

            // Used in RSRP and RSSI calculations -- this value is subtracted from the sum of power and gain, 
            // before the path loss is calculated and subtracted.
            // Default value if undefined: 2
            "cableLoss": 2,                  

            // Used in RSRP and RSSI calculations -- this value is subtracted from the sum of power and gain, 
            // before the path loss is calculated and subtracted.
            // If undefined or set to null, it will be automatically set depending on antenna type (10log(50)
            // for MobileNetwork, and 0 for ItsG5.
            "resourceBlockConstant": null    
                                             
        },
        // A few more station examples:
        {
            "id": "1255",
            "name": "Its2151",
            "center": {
                "x": 268283.4066450454,
                "y": 7040847.668103122 
            },
            "heightAboveTerrain": 3, 
            "maxRadius": 5000,
            "antennaType": "ItsG5",
            "power": 42,
        },
        {
            "id": "1256b",
            "name": "Another station",
            "center": {
                "x": 268283.4066450454,
                "y": 7040847.668103122 
            },
            "heightAboveTerrain": 3,
            "maxRadius": 5000,
            "antennaType": "ItsG5",
            "power": 15
        }
    ],

    // The directory where you want results to be stored.
    "OutputDirectory": "..\\RoadNetwork",    

    // If set to true, a shape file containing each calculated point and its results will be written
    // to the results directory.
    "WriteShape": true,                      
    // The filename of the shape file that is written if WriteShape is true.
    "ShapeFileName": "results.shp",          

    // If set to true, a CSV file containing each calculated point and its results will be written
    // to the results directory.
    "WriteCsv": true,                        
    // The filename of the CSV file that is written if WriteCsv is true.
    "CsvFileName": "results.csv",            
    // The separator between columns in the CSV file.
    "CsvSeparator": ";",                     

    // If set to true, a log file containing information about the calculation process will be written
    // to the results directory.
    "WriteLog": true,                        
    // The filename of the log file that is written if WriteLog is true.
    "LogFileName": "log.json",               

    // If set to true, various API result files will be written. The most important part of this is a 
    // GeoJSON file containing the data that can be displayed on the web-based map.
    "WriteApiResults": false,                
    // The name of the API results directory that will be written inside the results directory.
    "ApiResultInnerFolderName": "links",     

    // Settings for where elevation data is stored.
    "Terrain":                               
    {
        // Using an online cache means that the application will download tiles as requiered, convert them
        // to an optimized binary format, then store them in the defined location. This means that the 
        // application will spend a lot of time downloading elevation data every time you run it for a new
        // geographic area. 
        // You can use "LocalCache" if you have prepared elevation data yourself (see the 'Elevation data'
        // section in the documentation). This mode will only use already downloaded data.
        "Type": "OnlineCache",               
        // Dynamic url to elevation data WMS (allows downloading TIFF-files with elevation for the area 
        // defined in the url parameters).
        "WmsUrl": "https://wms.geonorge.no/skwms1/wms.hoyde-dom?bbox={0}&format=image/tiff&service=WMS&version=1.1.1&request=GetMap&srs=EPSG:25833&transparent=true&width={1}&height={2}&layers=dom1_33:None", 
        // Path where elevation data will be read from (and downloaded to if OnlineCache). A fast and 
        // large SSD is recommended.
        "Location": "I:\\Lambda\\Tiles_512", 
        // Setting for memory handling. This sets how many tiles can be kept in memory at the same time.
        // Too high number, and the application will crash when it is out of memory. Too low, and the
        // application will have to read files over and over again, resulting in longer calculation times.
        "MaxCacheItems": 300,                
        // Setting for memory handling. This sets how many of the least recently used tiles that are 
        // removed once the memory cache has been filled.
        "RemoveCacheItemsWhenFull": 100,     
        // The size of elevation data tiles that is used in this calculation. 512 is a good balance between
        // small files where a lot of them has to be opened, but they can be opened very quickly, and large
        // files that cover a larger area, takes a longer time to open, but a fewer number of files has to be opened.
        "TileSize": 512,                     
    }
}
```

### Grid
A method for running calculations in a grid (a square on the map) was implemented as part of performance testing, but as it was not a focus of this project, there was never developed a config system or runnable entry point for this type of calculation. The [FullRun/Grid](https://github.com/erlenddahl/lambdaroad-model/tree/main/LambdaModel.Tests/FullRun/Grid) unit tests in the unit test project shows how the grid calculations can be run.

### Single point
Single point calculations were implemented for easy testing in the map based tool (see 'Running path loss calculations from a base station to a point'). As it has no value for the command line application in this project, there was never developed a config system or runnable entry point for this type of calculation, except for the one through the REST API. For code examples on how to run single point calculations, see [Controllers/SinglePointController](https://github.com/erlenddahl/lambdaroad-model/blob/main/LambdaRestApi/Controllers/SinglePointController.cs) in the LambdaRestApi project.

## Elevation data
One of the goals of this project was to implement path loss models using highly detailed terrain profiles. The elevation data is therefore an important part of the project. If you run the online tool, it will use elevation data already prepared on the server. 

If you need to run offline calculations, you will need to download elevation data yourself. The easiest way is to configure the application to fetch elevation data by demand ("OnlineCache", see the documentation example above). When using this configuration, the application will automatically download all elevation data tiles it requires during calculations, and store them in the elevation data location (Terrain.Location in the configuration file). This means that the calculation may take a very long time the first time you run it, as it has to download a lot of elevation data.

A quicker way of running calculations is to prepare the elevation data manually beforehand. This can be done by downloading elevation data TIFF files from [hoydedata.no](https://hoydedata.no/LaserInnsyn/). Open the left side menu ("Nedlasting"), then pick the countrywide ("Landsdekkende") section, and download the sections of the country that is relevant. These zip files must be unpacked to a directory, and then the application can be run with a tile generation config file, which will parse the TIFF files and convert them to an optimized binary format that is much quicker to read while running the calculations.

Example tile generation config file:
```javascript
{
    Operation: "GenerateTiles",
    RawDataLocation: "C:\\directory\\with_unpacked_tiff_files",
    OutputDirectory: "C:\\directory\\for_finished_tiles",
    TileSize: 512
}
```

The latter method requires a bit of manual work, but will result in better performance when running calculations.
