using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Class provider of tiles.
/// </summary>
public class OnlineMapsProvider
{
    private const string SATELLITE = "Satellite";
    private const string RELIEF = "Relief";
    private const string TERRAIN = "Terrain";
    private const string MAP = "Map";

    private static OnlineMapsProvider[] providers;

    /// <summary>
    /// ID of provider
    /// </summary>
    public readonly string id;

    /// <summary>
    /// Human-readable provider title.
    /// </summary>
    public readonly string title;

    /// <summary>
    /// Indicates that the provider supports multilanguage.
    /// </summary>
    public bool? hasLanguage;

    /// <summary>
    /// Indicates that the provider supports a map with labels.
    /// </summary>
    public bool? hasLabels;

    /// <summary>
    /// Indicates that the label is always enabled.
    /// </summary>
    public bool? labelsEnabled;

    /// <summary>
    /// Map projection.
    /// </summary>
    public OnlineMapsProjection projection;

    /// <summary>
    /// Indicates that the provider uses HTTP.
    /// </summary>
    public bool? useHTTP;

    /// <summary>
    /// Index of current provider.
    /// </summary>
    public int index;

    /// <summary>
    /// Extension. Token {ext}, that is being replaced in the URL.
    /// </summary>
    public string ext;

    /// <summary>
    /// Property. Token {prop}, that is being replaced in the URL.
    /// </summary>
    public string prop;

    /// <summary>
    /// Indicates that the provider uses two letter language code.
    /// </summary>
    public bool twoLetterLanguage = true;

    public bool logUrl = false;

    private string _url;
    private MapType[] _types;

    /// <summary>
    /// Gets / sets the URL pattern of tiles.
    /// </summary>
    public string url
    {
        get { return _url; }
        set
        {
            _url = value;
            if (!value.StartsWith("https")) useHTTP = true;
        }
    }

    private OnlineMapsProvider(string title) : this(title.ToLower(), title)
    {
        
    }

    private OnlineMapsProvider(string id, string title)
    {
        this.id = id.ToLower();
        this.title = title;
        projection = new OnlineMapsProjectionSphericalMercator();
    }

    /// <summary>
    /// Array of map types available for the current provider.
    /// </summary>
    public MapType[] types
    {
        get { return _types; }
    }

    /// <summary>
    /// Gets an instance of a map type by ID.\n
    /// ID - providerID or providerID(dot)typeID.\n
    /// If the typeID is not specified returns the first map type of provider.
    /// If the provider ID is not found, returns the first map type of the first provider.
    /// Example: nokia or google.satellite
    /// </summary>
    /// <param name="mapTypeID">ID of map type</param>
    /// <returns>Instance of map type</returns>
    public static MapType FindMapType(string mapTypeID)
    {
        if (providers == null)  InitProviders();
        string[] parts = mapTypeID.Split('.');

        foreach (OnlineMapsProvider provider in providers)
        {
            if (provider.id == parts[0])
            {
                if (parts.Length == 1) return provider.types[0];
                foreach (MapType type in provider.types)
                {
                    if (type.id == parts[1]) return type;
                }
                return provider.types[0];
            }
        }
        return providers[0].types[0];
    }

    /// <summary>
    /// Gets map type by index.
    /// </summary>
    /// <param name="index">Index of map type.</param>
    /// <param name="repeat">TRUE - Repeat index value, FALSE - Clamp index value.</param>
    /// <returns>Instance of map type.</returns>
    public MapType GetByIndex(int index, bool repeat = false)
    {
        if (repeat) index = Mathf.RoundToInt(Mathf.Repeat(index, _types.Length - 1));
        else index = Mathf.Clamp(index, 0, _types.Length);
        return _types[index];
    }

    /// <summary>
    /// Gets array of providers
    /// </summary>
    /// <returns>Array of providers</returns>
    public static OnlineMapsProvider[] GetProviders()
    {
        if (providers == null) InitProviders();
        return providers;
    }

    /// <summary>
    /// Gets array of provider titles.
    /// </summary>
    /// <returns>Array of provider titles</returns>
    public static string[] GetProvidersTitle()
    {
        if (providers == null) InitProviders();
        return providers.Select(p => p.title).ToArray();
    }

    private static void InitProviders()
    {
        providers = new []
        {
            new OnlineMapsProvider("arcgis", "ArcGIS (Esri)")
            {
                url = "https://server.arcgisonline.com/ArcGIS/rest/services/{variant}/MapServer/tile/{zoom}/{y}/{x}",
                _types = new []
                {
                    new MapType("WorldImagery") { variantWithoutLabels = "World_Imagery" },
                    new MapType("WorldTopoMap") { variantWithLabels = "World_Topo_Map" },
                    new MapType("WorldStreetMap") { variantWithLabels = "World_Street_Map"},
                    new MapType("DeLorme") { variantWithLabels = "Specialty/DeLorme_World_Base_Map"},
                    new MapType("WorldTerrain") { variantWithoutLabels = "World_Terrain_Base"},
                    new MapType("WorldShadedRelief") { variantWithoutLabels = "World_Shaded_Relief"},
                    new MapType("WorldPhysical") { variantWithoutLabels = "World_Physical_Map"},
                    new MapType("OceanBasemap") { variantWithLabels = "Ocean_Basemap"},
                    new MapType("NatGeoWorldMap") { variantWithLabels = "NatGeo_World_Map"},
                    new MapType("WorldGrayCanvas") { variantWithLabels = "Canvas/World_Light_Gray_Base"},
                }
            },
            new OnlineMapsProvider("CartoDB")
            {
                url = "http://a.basemaps.cartocdn.com/{variant}/{z}/{x}/{y}.png",
                _types = new []
                {
                    new MapType("Positron")
                    {
                        variantWithLabels = "light_all",
                        variantWithoutLabels = "light_nolabels"
                    },
                    new MapType("DarkMatter")
                    {
                        variantWithLabels = "dark_all",
                        variantWithoutLabels = "dark_nolabels"
                    },
                }
            },
            new OnlineMapsProvider("google", "Google Maps")
            {
                hasLanguage = true,
                _types = new[]
                {
                    new MapType(SATELLITE)
                    {
                        urlWithLabels = "https://mt{rnd0-3}.googleapis.com/vt/lyrs=y&hl={lng}&x={x}&y={y}&z={zoom}",
                        urlWithoutLabels = "https://khm{rnd0-3}.googleapis.com/kh?v=196&hl={lng}&x={x}&y={y}&z={zoom}",
                    },
                    new MapType(RELIEF)
                    {
                        urlWithLabels = "https://mts{rnd0-3}.google.com/vt/lyrs=t@131,r@216000000&src=app&hl={lng}&x={x}&y={y}&z={zoom}&s="
                    },
                    new MapType(TERRAIN)
                    {
                        urlWithLabels = "https://mt{rnd0-3}.googleapis.com/vt?pb=!1m4!1m3!1i{zoom}!2i{x}!3i{y}!2m3!1e0!2sm!3i295124088!3m9!2s{lng}!3sUS!5e18!12m1!1e47!12m3!1e37!2m1!1ssmartmaps!4e0"
                    }
                }
            },
            new OnlineMapsProvider("Hydda")
            {
                url = "http://{s}.tile.openstreetmap.se/hydda/{variant}/{z}/{x}/{y}.png",
                _types = new []
                {
                    new MapType("Full") { variantWithLabels = "full" },
                    new MapType("Base") { variantWithLabels = "base" },
                    new MapType("RoadsAndLabels") { variantWithLabels = "roads_and_labels" },
                }
            },
            new OnlineMapsProvider("MapQuest")
            {
                url = "http://ttiles0{rnd1-4}.mqcdn.com/tiles/1.0.0/vy/{variant}/{zoom}/{x}/{y}.png",
                _types = new []
                {
                    new MapType(SATELLITE) { variantWithoutLabels = "sat" },
                    new MapType(TERRAIN) { variantWithLabels = "map" },
                    new MapType("HybridOverlay") { urlWithLabels = "http://otile{rnd1-4}.mqcdn.com/tiles/1.0.0/hyb/{z}/{x}/{y}.png" },
                }
            },
            new OnlineMapsProvider("mapy", "Mapy.CZ")
            {
                url = "https://m{rnd0-4}.mapserver.mapy.cz/{variant}/{zoom}-{x}-{y}",
                _types = new []
                {
                    new MapType(SATELLITE) { variantWithoutLabels = "ophoto-m" },
                    new MapType("Travel") { variantWithLabels = "wturist-m" }, 
                    new MapType("Winter") { variantWithLabels = "wturist_winter-m" }, 
                    new MapType("Geographic") { variantWithLabels = "zemepis-m" }, 
                    new MapType("Summer") { variantWithLabels = "turist_aquatic-m" }, 
                    new MapType("19century", "19th century") { variantWithLabels = "army2-m" }, 
                }
            }, 
            new OnlineMapsProvider("nokia", "Nokia Maps (here.com)")
            {
                url = "https://{rnd1-4}.maps.nlp.nokia.com/maptile/2.1/{prop}/newest/{variant}/{zoom}/{x}/{y}/256/png8?lg={lng}&app_id=xWVIueSv6JL0aJ5xqTxb&app_code=djPZyynKsbTjIUDOBcHZ2g",
                twoLetterLanguage = false,
                hasLanguage = true,
                labelsEnabled = true,
                prop = "maptile",

                _types = new []
                {
                    new MapType(SATELLITE)
                    {
                        variantWithLabels = "hybrid.day",
                        variantWithoutLabels = "satellite.day",
                    },
                    new MapType(TERRAIN)
                    {
                        variant = "terrain.day",
                        propWithoutLabels = "basetile",
                    },
                    new MapType(MAP)
                    {
                        variant = "normal.day",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalDayCustom")
                    {
                        variant = "normal.day.custom",
                        propWithoutLabels = "basetile",
                    }, 
                    new MapType("normalDayGrey")
                    {
                        variant = "normal.day.grey",
                        propWithoutLabels = "basetile",
                    }, 
                    new MapType("normalDayMobile")
                    {
                        variant = "normal.day.mobile",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalDayGreyMobile")
                    {
                        variant = "normal.day.grey.mobile",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalDayTransit")
                    {
                        variant = "normal.day.transit",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalDayTransitMobile")
                    {
                        variant = "normal.day.transit.mobile",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalNight")
                    {
                        variant = "normal.night",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalNightMobile")
                    {
                        variant = "normal.night.mobile",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalNightGrey")
                    {
                        variant = "normal.night.grey",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("normalNightGreyMobile")
                    {
                        variant = "normal.night.grey.mobile",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("carnavDayGrey")
                    {
                        variantWithLabels = "carnav.day.grey",
                        propWithoutLabels = "basetile",
                    },
                    new MapType("pedestrianDay")
                    {
                        variantWithLabels = "pedestrian.day"
                    }, 
                    new MapType("pedestrianNight")
                    {
                        variantWithLabels = "pedestrian.night"
                    }, 
                }
            },
            new OnlineMapsProvider("OpenMapSurfer")
            {
                url = "http://korona.geog.uni-heidelberg.de/tiles/{variant}/x={x}&y={y}&z={z}",
                _types = new []
                {
                    new MapType("Roads") { variantWithLabels = "roads" },
                    new MapType("AdminBounds") { variantWithLabels = "adminb" },
                    new MapType("Grayscale") { variantWithLabels = "roadsg" },
                }
            },
            new OnlineMapsProvider("osm", "OpenStreetMap")
            {
                _types = new []
                {
                    new MapType("Mapnik") { urlWithLabels = "https://a.tile.openstreetmap.org/{zoom}/{x}/{y}.png" },
                    new MapType("BlackAndWhite") { urlWithLabels = "http://a.tiles.wmflabs.org/bw-mapnik/{zoom}/{x}/{y}.png" },
                    new MapType("DE") { urlWithLabels = "http://a.tile.openstreetmap.de/tiles/osmde/{zoom}/{x}/{y}.png" },
                    new MapType("France") { urlWithLabels = "http://a.tile.openstreetmap.fr/osmfr/{zoom}/{x}/{y}.png" },
                    new MapType("HOT") { urlWithLabels = "http://a.tile.openstreetmap.fr/hot/{zoom}/{x}/{y}.png" },
                }
            },
            new OnlineMapsProvider("OpenTopoMap")
            {
                _types = new []
                {
                    new MapType("OpenTopoMap") { urlWithLabels = "http://a.tile.opentopomap.org/{z}/{x}/{y}.png" },
                }
            },
            new OnlineMapsProvider("OpenWeatherMap")
            {
                url = "http://a.tile.openweathermap.org/map/{variant}/{z}/{x}/{y}.png",
                _types = new []
                {
                    new MapType("Clouds") { variantWithoutLabels = "clouds" },
                    new MapType("CloudsClassic") { variantWithoutLabels = "clouds_cls" },
                    new MapType("Precipitation") { variantWithoutLabels = "precipitation" },
                    new MapType("PrecipitationClassic") { variantWithoutLabels = "precipitation_cls" },
                    new MapType("Rain") { variantWithoutLabels = "rain" },
                    new MapType("RainClassic") { variantWithoutLabels = "rain_cls" },
                    new MapType("Pressure") { variantWithoutLabels = "pressure" },
                    new MapType("PressureContour") { variantWithoutLabels = "pressure_cntr" },
                    new MapType("Wind") { variantWithoutLabels = "wind" },
                    new MapType("Temperature") { variantWithoutLabels = "temp" },
                    new MapType("Snow") { variantWithoutLabels = "snow" },
                }
            },
            new OnlineMapsProvider("Stamen")
            {
                url = "https://stamen-tiles-a.a.ssl.fastly.net/{variant}/{z}/{x}/{y}.png",
                _types = new []
                {
                    new MapType("Toner") { variantWithLabels = "toner" },
                    new MapType("TonerBackground") { variantWithoutLabels = "toner-background" },
                    new MapType("TonerHybrid") { variantWithLabels = "toner-hybrid" },
                    new MapType("TonerLines") { variantWithLabels = "toner-lines" },
                    new MapType("TonerLabels") { variantWithLabels = "toner-labels" },
                    new MapType("TonerLite") { variantWithLabels = "toner-lite" },
                    new MapType("Watercolor") { variantWithoutLabels = "watercolor" },
                }
            },
            new OnlineMapsProvider("Thunderforest")
            {
                url = "https://a.tile.thunderforest.com/{variant}/{z}/{x}/{y}.png",
                _types = new []
                {
                    new MapType("OpenCycleMap") { variantWithLabels = "cycle" },
                    new MapType("Transport") { variantWithLabels = "transport" },
                    new MapType("TransportDark") { variantWithLabels = "transport-dark" },
                    new MapType("SpinalMap") { variantWithLabels = "spinal-map" },
                    new MapType("Landscape") { variantWithLabels = "landscape" },
                    new MapType("Outdoors") { variantWithLabels = "outdoors" },
                    new MapType("Pioneer") { variantWithLabels = "pioneer" },
                }
            },
            new OnlineMapsProvider("TianDiTu")
            {
                _types = new []
                {
                    new MapType("Normal")
                    {
                        urlWithoutLabels = "http://t{rnd0-7}.tianditu.cn/DataServer?T=vec_w&X={x}&Y={y}&L={z}"
                    },
                    new MapType(SATELLITE)
                    {
                        urlWithoutLabels = "http://t{rnd0-7}.tianditu.cn/DataServer?T=img_w&X={x}&Y={y}&L={z}"
                    },
                    new MapType(TERRAIN)
                    {
                        urlWithoutLabels = "http://t{rnd0-7}.tianditu.cn/DataServer?T=ter_w&X={x}&Y={y}&L={z}"
                    },
                }
            },
            new OnlineMapsProvider("virtualearth", "Virtual Earth (Bing Maps)")
            {
                hasLanguage = true,
                _types = new []
                {
                    new MapType("Aerial")
                    {
                        urlWithoutLabels = "http://ak.t{rnd0-4}.tiles.virtualearth.net/tiles/a{quad}.jpeg?mkt={lng}&g=1457&n=z",
                        urlWithLabels = "http://ak.dynamic.t{rnd0-4}.tiles.virtualearth.net/comp/ch/{quad}?mkt={lng}&it=A,G,L,LA&og=30&n=z"
                    },
                    new MapType("Road")
                    {
                        urlWithLabels = "http://ak.dynamic.t{rnd0-4}.tiles.virtualearth.net/comp/ch/{quad}?mkt={lng}&it=G,VE,BX,L,LA&og=30&n=z"
                    }
                }
            },
            new OnlineMapsProvider("yandex", "Yandex Maps")
            {
                projection = new OnlineMapsProjectionWGS84(),
                _types = new []
                {
                    new MapType(MAP)
                    {
                        hasLanguage = true,
                        urlWithLabels = "https://vec0{rnd1-4}.maps.yandex.net/tiles?l=map&v=4.65.1&x={x}&y={y}&z={zoom}&scale=1&lang={lng}"
                    }, 
                    new MapType(SATELLITE)
                    {
                        urlWithoutLabels = "https://sat0{rnd1-4}.maps.yandex.net/tiles?l=sat&v=3.261.0&x={x}&y={y}&z={zoom}"
                    }, 
                }
            }, 
            new OnlineMapsProvider("Other")
            {
                _types = new []
                {
                    new MapType("AMap Satellite") { urlWithoutLabels = "https://webst02.is.autonavi.com/appmaptile?style=6&x={x}&y={y}&z={zoom}" },
                    new MapType("AMap Terrain") { urlWithLabels = "https://webrd03.is.autonavi.com/appmaptile?lang=zh_cn&size=1&scale=1&style=8&x={x}&y={y}&z={zoom}" },
                    new MapType("MtbMap") { urlWithLabels = "http://tile.mtbmap.cz/mtbmap_tiles/{z}/{x}/{y}.png" },
                    new MapType("HikeBike") { urlWithLabels = "http://a.tiles.wmflabs.org/hikebike/{z}/{x}/{y}.png" },
                }
            }, 
            new OnlineMapsProvider("Custom")
            {
                _types = new [] 
                {
                    new MapType("Custom") { isCustom = true }
                }
            }
        };

        for (int i = 0; i < providers.Length; i++)
        {
            OnlineMapsProvider provider = providers[i];
            provider.index = i;
            for (int j = 0; j < provider._types.Length; j++)
            {
                MapType type = provider._types[j];
                type.provider = provider;
                type.fullID = provider.id + "." + type.id;
                type.index = j;
            }
        }
    }

    public static string Upgrade(int providerID, int type)
    {
        StringBuilder builder = new StringBuilder();
        if (providerID == 0) builder.Append("arcgis");
        else if (providerID == 1) builder.Append("google");
        else if (providerID == 2) builder.Append("nokia");
        else if (providerID == 3) builder.Append("mapquest");
        else if (providerID == 4) builder.Append("virtualearth");
        else if (providerID == 5) builder.Append("osm");
        else if (providerID == 6) builder.Append("sputnik");
        else if (providerID == 7) builder.Append("amap");
        else if (providerID == 999) builder.Append("custom").Append(".").Append("custom");
        else
        {
            Debug.LogWarning("Trying to upgrade provider failed. Please select the provider manually.");
            return "arcgis";
        }

        string[] availableTypes = OnlineMaps.GetAvailableTypes((OnlineMapsProviderEnum)providerID);

        if (providerID < 8 && availableTypes.Length > type)
        {
            builder.Append(".");
            builder.Append(availableTypes[type].ToLower());
        }
        
        return builder.ToString();
    }

    /// <summary>
    /// Class of map type
    /// </summary>
    public class MapType
    {
        /// <summary>
        /// ID of map type
        /// </summary>
        public readonly string id;

        public string fullID;

        /// <summary>
        /// Human-readable map type title.
        /// </summary>
        public readonly string title;

        /// <summary>
        /// Reference to provider instance.
        /// </summary>
        public OnlineMapsProvider provider;

        /// <summary>
        /// Index of map type
        /// </summary>
        public int index;

        /// <summary>
        /// Indicates that this is an custom provider.
        /// </summary>
        public bool isCustom;

        private bool hasWithoutLabels = false;
        private bool hasWithLabels = false;

        private string _ext;
        private bool? _hasLanguage;
        private bool? _hasLabels;
        private bool? _labelsEnabled;
        private string _urlWithLabels;
        private string _urlWithoutLabels;
        private bool? _useHTTP;
        private string _variantWithLabels;
        private string _variantWithoutLabels;
        private string _propWithLabels;
        private string _propWithoutLabels;
        private bool? _logUrl;

        /// <summary>
        /// Extension. Token {ext}, that is being replaced in the URL.
        /// </summary>
        public string ext
        {
            get
            {
                if (!string.IsNullOrEmpty(_ext)) return _ext;
                if (!string.IsNullOrEmpty(provider.ext)) return provider.ext;
                return string.Empty;
            }
            set { _ext = value; }
        }

        /// <summary>
        /// Indicates that the map type supports multilanguage.
        /// </summary>
        public bool hasLanguage
        {
            get
            {
                if (_hasLanguage.HasValue) return _hasLanguage.Value;
                if (provider.hasLanguage.HasValue) return provider.hasLanguage.Value;
                return false;
            }
            set { _hasLanguage = value; }
        }

        /// <summary>
        /// Indicates that the provider supports a map with labels.
        /// </summary>
        public bool hasLabels
        {
            get
            {
                if (_hasLabels.HasValue) return _hasLabels.Value;
                if (provider.hasLabels.HasValue) return provider.hasLabels.Value;
                return false;
            }
            set { _hasLabels = value; }
        }

        /// <summary>
        /// Indicates that the label is always enabled.
        /// </summary>
        public bool labelsEnabled
        {
            get
            {
                if (_labelsEnabled.HasValue) return _labelsEnabled.Value;
                if (provider.labelsEnabled.HasValue) return provider.labelsEnabled.Value;
                return false;
            }
            set { _labelsEnabled = value; }
        }

        public bool logUrl
        {
            get
            {
                if (_logUrl.HasValue) return _logUrl.Value;
                return provider.logUrl;
            }
            set { _logUrl = value; }
        }

        /// <summary>
        /// Property. Token {prop} when label enabled, that is being replaced in the URL.
        /// </summary>
        public string propWithLabels
        {
            get
            {
                if (!string.IsNullOrEmpty(_propWithLabels)) return _propWithLabels;
                return provider.prop;
            }
            set
            {
                _propWithLabels = value;
                labelsEnabled = true;
                hasWithLabels = true;
                if (hasWithoutLabels) hasLabels = true;
            }
        }

        /// <summary>
        /// Property. Token {prop} when label disabled, that is being replaced in the URL.
        /// </summary>
        public string propWithoutLabels
        {
            get
            {
                if (!string.IsNullOrEmpty(_propWithoutLabels)) return _propWithoutLabels;
                return provider.prop;
            }
            set
            {
                _propWithoutLabels = value;
                hasWithoutLabels = true;
                if (hasWithLabels) hasLabels = true;
            }
        }

        /// <summary>
        /// Variant. Token {variant}, that is being replaced in the URL.
        /// </summary>
        public string variant
        {
            set
            {
                _variantWithoutLabels = value;
                _variantWithLabels = value;
                hasLabels = true;
                hasWithLabels = true;
                hasWithoutLabels = true;
                labelsEnabled = true;
            }
        }

        /// <summary>
        /// Variant. Token {variant} when label enabled, that is being replaced in the URL.
        /// </summary>
        public string variantWithLabels
        {
            get { return _variantWithLabels; }
            set
            {
                _variantWithLabels = value;
                labelsEnabled = true;
                hasWithLabels = true;
                if (hasWithoutLabels) hasLabels = true;
            }
        }

        /// <summary>
        /// Variant. Token {variant} when label disabled, that is being replaced in the URL.
        /// </summary>
        public string variantWithoutLabels
        {
            get { return _variantWithoutLabels; }
            set
            {
                _variantWithoutLabels = value;
                hasWithoutLabels = true;
                if (hasWithLabels) hasLabels = true;
            }
        }

        /// <summary>
        /// Gets / sets the URL pattern of tiles with labels.
        /// </summary>
        public string urlWithLabels
        {
            get { return _urlWithLabels; }
            set
            {
                _urlWithLabels = value;
                labelsEnabled = true;
                hasWithLabels = true;
                if (hasWithoutLabels) hasLabels = true;
                if (!value.StartsWith("https")) _useHTTP = true;
            }
        }

        /// <summary>
        /// Gets / sets the URL pattern of tiles without labels.
        /// </summary>
        public string urlWithoutLabels
        {
            get { return _urlWithoutLabels; }
            set
            {
                _urlWithoutLabels = value;
                hasWithoutLabels = true;
                if (hasWithLabels) hasLabels = true;
                if (!value.StartsWith("https")) _useHTTP = true;
            }
        }

        /// <summary>
        /// Indicates that the map type uses HTTP.
        /// </summary>
        public bool useHTTP
        {
            get
            {
                if (_useHTTP.HasValue) return _useHTTP.Value;
                if (provider.useHTTP.HasValue) return provider.useHTTP.Value;
                return false;
            }
            set { _useHTTP = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Human-readable map type title.</param>
        public MapType(string title):this(title.ToLower(), title)
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">ID of map type.</param>
        /// <param name="title">Human-readable map type title.</param>
        public MapType(string id, string title)
        {
            this.id = id;
            this.title = title;
        }

        /// <summary>
        /// Gets the URL to download the tile.
        /// </summary>
        /// <param name="tile">Instence of tile.</param>
        /// <returns>URL to download the tile.</returns>
        public string GetURL(OnlineMapsTile tile)
        {
            bool useLabels = hasLabels ? tile.labels : labelsEnabled;
            if (useLabels)
            {
                if (!string.IsNullOrEmpty(_urlWithLabels)) return GetURL(tile, _urlWithLabels, true);
                if (!string.IsNullOrEmpty(provider.url)) return GetURL(tile, provider.url, true);
                return GetURL(tile, _urlWithoutLabels, false);
            }

            if (!string.IsNullOrEmpty(_urlWithoutLabels)) return GetURL(tile, _urlWithoutLabels, false);
            if (!string.IsNullOrEmpty(provider.url)) return GetURL(tile, provider.url, false);
            return GetURL(tile, _urlWithLabels, true);
        }

        private string GetURL(OnlineMapsTile tile, string url, bool labels)
        {
            url = Regex.Replace(url, @"{\w+}", delegate(Match match)
            {
                string v = match.Value.ToLower().Trim('{', '}');

                if (OnlineMapsTile.OnReplaceURLToken != null)
                {
                    string ret = OnlineMapsTile.OnReplaceURLToken(tile, v);
                    if (ret != null) return ret;
                }

                if (v == "zoom") return tile.zoom.ToString();
                if (v == "z") return tile.zoom.ToString();
                if (v == "x") return tile.x.ToString();
                if (v == "y") return tile.y.ToString();
                if (v == "quad") return OnlineMapsUtils.TileToQuadKey(tile.x, tile.y, tile.zoom);
                if (v == "lng") return tile.language;
                if (v == "ext") return ext;
                if (v == "prop") return labels ? propWithLabels : propWithoutLabels;
                if (v == "variant") return labels ? variantWithLabels : variantWithoutLabels;
                return v;
            });
            url = Regex.Replace(url, @"{rnd(\d+)-(\d+)}", delegate(Match match)
            {
                int v1 = int.Parse(match.Groups[1].Value);
                int v2 = int.Parse(match.Groups[2].Value);
                return Random.Range(v1, v2 + 1).ToString();
            });
            if (logUrl) Debug.Log(url);
            return url;
        }

        public override string ToString()
        {
            return fullID;
        }
    }
}