
data:extend({
    -- runtime-global
    { type = "bool-setting", name = "game-speed-only-allow-admins", setting_type = "runtime-global", default_value = true, },
    -- runtime-per-user
    { type = "bool-setting", name = "game-speed-gui-open-initially", setting_type = "runtime-per-user", default_value = true, order = "a", },
    { type = "int-setting", name = "game-speed-gui-initial-position-x", setting_type = "runtime-per-user", default_value = 160, minimum_value = -10000, maximum_value = 10000, order = "b", },
    { type = "int-setting", name = "game-speed-gui-initial-position-y", setting_type = "runtime-per-user", default_value = 360, minimum_value = -10000, maximum_value = 10000, order = "c", },
    { type = "bool-setting", name = "game-speed-show-play-until-time-tab-initially", setting_type = "runtime-per-user", default_value = true, order = "d", },
    { type = "bool-setting", name = "game-speed-show-play-for-limited-time-tab-initially", setting_type = "runtime-per-user", default_value = true, order = "e", },
    { type = "bool-setting", name = "game-speed-show-ticks-initially", setting_type = "runtime-per-user", default_value = true, order = "f", },
})
