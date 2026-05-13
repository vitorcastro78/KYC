namespace KYC.Infrastructure.ExternalSources;

/// <summary>Mapeamento parcial Wikidata P17 (item país) → ISO 3166-1 alpha-2 (ampliável).</summary>
internal static class WikidataCountryIso
{
    private static readonly Dictionary<int, string> NumericIdToIso = new()
    {
        [30] = "US", [31] = "CA", [45] = "PT", [77] = "UY", [79] = "EG", [96] = "MX", [100] = "BG",
        [114] = "KE", [142] = "FR", [145] = "SY", [148] = "CN",         [155] = "BR", [159] = "RU", [183] = "GA", [189] = "IE", [191] = "EE", [211] = "LV", [212] = "UA",
        [214] = "SK", [215] = "SI", [217] = "MD", [218] = "RO",
        [219] = "BG", [222] = "AL", [224] = "HR", [225] = "BA", [227] = "MK", [228] = "GR", [230] = "GE",
        [232] = "KZ", [235] = "MC", [236] = "ME", [238] = "SM", [241] = "LU", [252] = "ID", [258] = "ZA",
        [262] = "SZ", [265] = "MU", [298] = "CL", [347] = "LI", [399] = "AM", [403] = "RS",
        [408] = "AU", [414] = "AR", [419] = "PE", [423] = "KP", [424] = "KH", [458] = "SG", [657] = "CH",
        [664] = "DO", [668] = "IN", [672] = "IL", [686] = "VU", [695] = "SB", [697] = "NR", [702] = "FM",
        [709] = "MH", [710] = "KI", [711] = "TV", [714] = "FJ", [717] = "WS", [733] = "PY", [734] = "GY",
        [736] = "EC", [739] = "CO", [750] = "BO", [754] = "TT", [756] = "SR", [757] = "VC",
        [760] = "LC", [763] = "KN", [766] = "JM", [774] = "GT", [778] = "BZ", [781] = "AG",
        [783] = "HN", [784] = "NI", [786] = "CR", [790] = "VE", [792] = "SV", [796] = "SA", [804] = "PA",
        [813] = "FK", [833] = "MY", [836] = "MM", [842] = "OM", [843] = "PK",
        [851] = "SA", [826] = "MV", [858] = "SY", [863] = "NP", [865] = "TW", [869] = "TH", [878] = "AE",
        [881] = "VN",
        [884] = "KR", [889] = "AF", [902] = "BD", [916] = "AO", [928] = "PH", [953] = "ZM",
        [954] = "ZW", [974] = "TL", [983] = "ST", [986] = "ER", [1006] = "BJ", [1007] = "BF", [1008] = "CI",
        [1009] = "GH", [1010] = "SN", [1011] = "NE", [1013] = "LR", [1014] = "SL", [1015] = "GN", [1016] = "GW",
        [1017] = "CV", [1019] = "MR", [1020] = "ML", [1025] = "DZ", [1027] = "TN", [1028] = "MA", [1029] = "LY",
        [1030] = "GA", [1032] = "TD", [1033] = "NG", [1035] = "CM", [1036] = "GQ", [1037] = "CF", [1039] = "SD",
        [1041] = "ER", [1042] = "DJ", [1044] = "SO", [1047] = "UG", [1048] = "TZ", [1049] = "RW", [1050] = "BI",
        [1161] = "NA", [1162] = "MW", [1163] = "MZ", [1165] = "BW", [1166] = "LS", [1167] = "SZ", [1173] = "CD",
        [1174] = "CG", [1183] = "MG", [1184] = "KM", [1186] = "SC", [1187] = "MU", [125238] = "CW",
        [26202] = "AW", [26232] = "CW", [26267] = "BQ", [26273] = "SX"
    };

    public static bool TryGetIso2(string? wikidataCountryId, out string iso2)
    {
        iso2 = string.Empty;
        if (string.IsNullOrEmpty(wikidataCountryId) || wikidataCountryId[0] != 'Q' ||
            !int.TryParse(wikidataCountryId.AsSpan(1), out var num))
            return false;
        if (!NumericIdToIso.TryGetValue(num, out var code))
            return false;
        iso2 = code;
        return true;
    }
}
