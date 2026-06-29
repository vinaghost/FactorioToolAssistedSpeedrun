local styles = data.raw["gui-style"].default

styles["game_speed_control_flow"] = {
    type = "horizontal_flow_style",
    parent = "horizontal_flow",
    vertical_align = "center",
}

styles["game_speed_horizontal_space"] = {
    type = "empty_widget_style",
    horizontally_stretchable = "on",
}

styles["game_speed_very_short_number_textfield"] = {
    type = "textbox_style",
    parent = "very_short_number_textfield",
    width = 35,
    horizontal_align = "right",
}

styles["game_speed_number_textfield"] = {
    type = "textbox_style",
    parent = "textbox",
    minimal_width = 60,
    horizontal_align = "right",
    horizontally_stretchable = "on",
}

-- from Raiguard's style guide
styles["game_speed_title_bar_draggable_space"] = {
    type = "empty_widget_style",
    parent = "draggable_space_header",
    horizontally_stretchable = "on",
    vertically_stretchable = "on",
    height = 24,
    right_margin = 4,
}

--[[
Copied from data/core/prototypes/style.lua
then modified from slot_sized_button with added tint on clicked_graphical_set.base
Completely scuffed.
--]]

local default_dirt_color = {15, 7, 3, 100}
local default_glow_color = {225, 177, 106, 255}

local function offset_by_2_default_glow(tint_value, scale_value)
    return
    {
        position = {280, 736},
        corner_size = 16,
        tint = tint_value,
        scale = scale_value,
        top_outer_border_shift = 4,
        bottom_outer_border_shift = -4,
        left_outer_border_shift = 4,
        right_outer_border_shift = -4,
        draw_type = "outer"
    }
end

styles["game_speed_selected_slot_sized_button"] = {
    type = "button_style",
    parent = "button",
    default_graphical_set =
    {
        base = {position = {363, 744}, corner_size = 8},
        shadow = offset_by_2_default_glow(default_dirt_color, 0.5)
    },
    disabled_graphical_set =
    {
        base = {position = {329, 744}, corner_size = 8},
        shadow = offset_by_2_default_glow(default_dirt_color, 0.5)
    },
    hovered_graphical_set =
    {
        base = {position = {346, 744}, corner_size = 8},
        shadow = offset_by_2_default_glow(default_dirt_color, 0.5),
        glow = offset_by_2_default_glow(default_glow_color, 0.5)
    },
    left_click_sound = {{ filename = "__core__/sound/gui-square-button.ogg", volume = 1 }},
    clicked_graphical_set =
    {
        base = {position = {363, 744}, tint = {255, 255, 255, 0}, corner_size = 8},
        shadow = offset_by_2_default_glow(default_dirt_color, 0.5),
    },
    size = 40,
    padding = 0
}

-- Sprites

-- properties copied from core/prototypes/utility-sprites.lua
-- Settings icons are from flib (Factorio Library)
-- 'Disabled' icons are regular icons inverted and with half transparency.

data:extend({
    {
        type = "sprite",
        name = "game_speed_settings_icon_black",
        filename = "__game-speed__/graphics/settings-icons.png",
        position = { 0, 0 },
        size = 32,
        flags = { "icon" },
    },
    {
        type = "sprite",
        name = "game_speed_settings_icon_white",
        filename = "__game-speed__/graphics/settings-icons.png",
        position = { 32, 0 },
        size = 32,
        flags = { "icon" },
    },
    {
        type = "sprite",
        name = "game_speed_max_speed",
        filename = "__game-speed__/graphics/max-speed.png",
        priority = "medium",
        width = 32,
        height = 32,
        mipmap_count = 2,
        flags = {"gui-icon"},
        scale = 0.5,
    },
    {
        type = "sprite",
        name = "game_speed_play_until",
        filename = "__game-speed__/graphics/play-until.png",
        priority = "medium",
        width = 32,
        height = 32,
        mipmap_count = 2,
        flags = {"gui-icon"},
        scale = 0.5,
    },
    {
        type = "sprite",
        name = "game_speed_play_until_disabled",
        filename = "__game-speed__/graphics/play-until-disabled.png",
        priority = "medium",
        width = 32,
        height = 32,
        mipmap_count = 2,
        flags = {"gui-icon"},
        scale = 0.5,
    },
    {
        type = "sprite",
        name = "game_speed_fast_forward_until",
        filename = "__game-speed__/graphics/fast-forward-until.png",
        priority = "medium",
        width = 32,
        height = 32,
        mipmap_count = 2,
        flags = {"gui-icon"},
        scale = 0.5,
    },
    {
        type = "sprite",
        name = "game_speed_fast_forward_until_disabled",
        filename = "__game-speed__/graphics/fast-forward-until-disabled.png",
        priority = "medium",
        width = 32,
        height = 32,
        mipmap_count = 2,
        flags = {"gui-icon"},
        scale = 0.5,
    },
})


-- Shortcut

data.raw.shortcut["game-speed-toggle-gui"] = {
    type = "shortcut",
    name = "game-speed-toggle-gui",
    toggleable = true,
    action = "lua",
    localised_name = {"game-speed.toggle-gui"},
    associated_control_input = "game-speed-toggle-gui",
    icon = "__core__/graphics/time-editor-icon.png",
    icon_size = 32,
    small_icon = "__core__/graphics/time-editor-icon.png",
    small_icon_size = 32,
}


-- Hotkeys

local function add_hotkey(name, key_sequence, alternative_key_sequence, order)
    data:extend({
        {
            type = "custom-input",
            name = name,
            key_sequence = key_sequence,
            alternative_key_sequence = alternative_key_sequence,
            consuming = "game-only",
            order = order,
        }
    })
end

add_hotkey("game-speed-toggle-gui", "CONTROL + S", "", "a")
add_hotkey("game-speed-reset-speed", "CONTROL + 0", "SHIFT + KP_MULTIPLY", "b")
add_hotkey("game-speed-speed-down", "CONTROL + MINUS", "SHIFT + KP_MINUS", "c")
add_hotkey("game-speed-play-pause", "CONTROL + SPACE", "KP_0", "d")
add_hotkey("game-speed-speed-up", "CONTROL + EQUALS", "SHIFT + KP_PLUS", "e")
add_hotkey("game-speed-max-speed", "CONTROL + SHIFT + EQUALS", "", "f")
add_hotkey("game-speed-tick-custom", "", "", "g")
add_hotkey("game-speed-tick-sixty", "", "", "h")
add_hotkey("game-speed-tick-once", "PERIOD", "KP_PERIOD", "i")

