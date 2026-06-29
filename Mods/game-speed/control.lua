require("util")

-- enum
-- not actually used anymore, just here for reference
local play_pause_state = {
    play = 1,
    -- tick_paused = false
    -- ticks_to_run = 0
    -- play/pause button displays 'pause' and is highlighted
    -- pressing the button makes the state go to 'pause'

    tick_until = 2,
    -- tick_paused = true
    -- ticks_to_run > 0
    -- play/pause button displays 'play' and is highlighted
    -- pressing the button makes the state go to 'play'
    -- when the tick until finished, the stage goes to 'pause'

    pause = 3,
    -- tick_paused = true
    -- ticks_to_run = 0
    -- play/pause button displays 'play' and is not highlighted
    -- pressing the button makes the state go to 'play'
}

-- constant
local fast_forward_speed = 3600
local speed_min = 0.25
local speed_max = 64

local function add_thousands_separators(num)
    local res = ""
    while num > 0 do
        if res ~= "" then
            res = " " .. res
        end
        local cur = num % 1000
        num = (num - cur) / 1000
        if num == 0 then
            res = cur .. res
        else
            res = string.format("%03d", cur) .. res
        end
    end
    return res
end

-- update the 'current time' label for the given player, or for all players if player_index == nil
local function update_current_time_label_internal(current_tick, player_index)
    local tick = current_tick % 60
    local total_seconds = (current_tick - tick) / 60
    local second = total_seconds % 60
    local total_minutes = (total_seconds - second) / 60
    local minute = total_minutes % 60
    local total_hours = (total_minutes - minute) / 60
    local hour = total_hours

    local caption_with_ticks = string.format("%d:%02d:%02d (%02d)", hour, minute, second, tick)
    local caption_without_ticks = string.format("%d:%02d:%02d", hour, minute, second)
    local tooltip = {"", {"game-speed.current-tick"}, ": ", add_thousands_separators(current_tick)}

    local function do_update(player_index)
        local refs = storage.player_info[player_index].refs
        local label = refs.current_time_label
        local show_ticks = refs.show_ticks_checkbox.state
        if show_ticks then
            label.caption = caption_with_ticks
        else
            label.caption = caption_without_ticks
        end
        label.tooltip = tooltip
    end

    if player_index then
        do_update(player_index)
    else
        for player_index, player_info in pairs(storage.player_info) do
            do_update(player_index)
        end
    end
end

local function update_current_time_label(player_index, current_tick)
    update_current_time_label_internal(current_tick, player_index)
end

local function update_current_time_label_for_all_players(current_tick)
    update_current_time_label_internal(current_tick)
end

-- function declarations
local update_show_ticks = nil
local update_permissions = nil

-- build the gui and set/reset refs
local function build_gui(player_index)
    local player = game.players[player_index]
    local screen = player.gui.screen

    storage.player_info[player_index] = {
        -- references to gui objects belonging to this player
        refs = {},
    }
    local refs = storage.player_info[player_index].refs

    local main_frame = screen.add{ type = "frame", direction = "vertical", }
    refs.main_frame = main_frame

    -- add title bar (from raiguard's style guide)
    do
        local title_bar = main_frame.add{ type = "flow", direction = "horizontal", name = "title_bar", }
        title_bar.drag_target = main_frame
        title_bar.add{ type = "label", style = "frame_title", caption = {"game-speed.title"}, ignored_by_interaction = true, }
        title_bar.add{ type = "empty-widget", style = "game_speed_title_bar_draggable_space", ignored_by_interaction = true, }
        refs.toggle_options_button = title_bar.add{ type = "sprite-button", style = "frame_action_button", sprite = "game_speed_settings_icon_white", hovered_sprite = "game_speed_settings_icon_black", clicked_sprite = "game_speed_settings_icon_black", }
        refs.main_frame_close_button = title_bar.add{ type = "sprite-button", style = "frame_action_button", sprite = "game_speed_settings_icon_white", hovered_sprite = "game_speed_settings_icon_black", clicked_sprite = "game_speed_settings_icon_black", }
    end

    local main_table = main_frame.add{ type = "table", style = "bordered_table", column_count = 1, }

    local function make_textfield_spec(style, default_text)
        return {
            type = "textfield",
            style = style,
            text = default_text,
            numeric = true,
            allow_decimal = false,
            allow_negative = false,
            lose_focus_on_confirm = true,
        }
    end

    -- Game speed controls
    do 
        local flow = main_table.add{ type = "flow", direction = "vertical" }
        flow.add{ type = "label", style = "caption_label", caption = {"gui-map-editor-time-editor.speed"}, }
        local display_flow = flow.add{ type = "flow", direction = "horizontal" }
        display_flow.add{ type = "label", caption = {"", {"gui-map-editor-time-editor.current-speed"}, ":"}, }
        display_flow.add{ type = "empty-widget", style = "game_speed_horizontal_space", }
        refs.game_speed_label = display_flow.add{ type = "label", caption = {"gui-speed-panel.normal"}, }
        local controls_flow = flow.add{ type = "flow", style = "game_speed_control_flow", direction = "horizontal", }
        refs.game_speed_controls_flow = controls_flow
        controls_flow.add{ type = "empty-widget", style = "game_speed_horizontal_space", }
        refs.reset_speed_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "utility/reset", tooltip = {"game-speed.reset-speed"}, }
        refs.speed_down_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "utility/speed_down", tooltip = {"game-speed.speed-down"}, }
        refs.play_pause_button = controls_flow.add{ type = "sprite-button", style = "game_speed_selected_slot_sized_button", sprite = "utility/pause", tooltip = {"game-speed.pause"}, }
        refs.speed_up_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "utility/speed_up", tooltip = {"game-speed.speed-up"}, }
        refs.max_speed_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "game_speed_max_speed", tooltip = {"game-speed.max-speed"}, }
    end

    -- 'Tick until' controls
    do 
        local flow = main_table.add{ type = "flow", direction = "vertical" }
        refs.tick_until_flow = flow
        flow.add{ type = "label", style = "caption_label", caption = {"game-speed.play-until-time"}, }
        local display_flow = flow.add{ type = "flow", direction = "horizontal" }
        display_flow.add{ type = "label", caption = {"", {"game-speed.current-time"}, ":"}, }
        display_flow.add{ type = "empty-widget", style = "game_speed_horizontal_space", }
        refs.current_time_label = display_flow.add{ type = "label", caption = "0:00:00 (00)" }

        local controls_flow = flow.add{ type = "flow", style = "game_speed_control_flow", direction = "horizontal", }
        refs.tick_until_controls_flow = controls_flow
        controls_flow.add{ type = "empty-widget", style = "game_speed_horizontal_space", }
        --controls_flow.add{ type = "label", caption = "H, M, S, tick:" }
        refs.until_hour_textfield = controls_flow.add(make_textfield_spec("game_speed_very_short_number_textfield", 0))
        controls_flow.add{ type = "label", caption = ":" }
        refs.until_minute_textfield = controls_flow.add(make_textfield_spec("game_speed_very_short_number_textfield", "00"))
        controls_flow.add{ type = "label", caption = ":" }
        refs.until_second_textfield = controls_flow.add(make_textfield_spec("game_speed_very_short_number_textfield", "00"))
        refs.until_tick_input_elements = {
            controls_flow.add{ type = "label", caption = "(" },
            controls_flow.add(make_textfield_spec("game_speed_very_short_number_textfield", "00")),
            controls_flow.add{ type = "label", caption = ")" },
        }
        refs.until_tick_textfield = refs.until_tick_input_elements[2]
        refs.play_until_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "game_speed_play_until_disabled", tooltip = {"game-speed.play-until"}, enabled = false, }
        refs.fast_forward_until_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "game_speed_fast_forward_until_disabled", tooltip = {"game-speed.fast-forward-until"}, enabled = false, }
    end

    -- 'Tick for' controls
    do 
        local flow = main_table.add{ type = "flow", direction = "vertical" }
        refs.tick_for_flow = flow
        flow.add{ type = "label", style = "caption_label", caption = {"gui-map-editor-time-editor.play-for-limited-time"}, }
        local controls_flow = flow.add{ type = "flow", style = "game_speed_control_flow", direction = "horizontal", }
        refs.tick_for_controls_flow = controls_flow
        --controls_flow.add{ type = "empty-widget", style = "game_speed_horizontal_space", }
        refs.tick_for_textfield = controls_flow.add(make_textfield_spec("game_speed_number_textfield", 3600))
        refs.tick_custom_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "utility/tick_custom", tooltip = {"game-speed.tick-custom"}, }
        refs.tick_sixty_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "utility/tick_sixty", tooltip = {"game-speed.tick-sixty"}, }
        refs.tick_once_button = controls_flow.add{ type = "sprite-button", style = "slot_sized_button", sprite = "utility/tick_once", tooltip = {"game-speed.tick-once"}, }
    end

    -- Options menu
    do
        local frame = screen.add{ type = "frame", direction = "vertical", visible = false, }
        refs.options_frame = frame
        frame.force_auto_center()

        -- add title bar (from raiguard's style guide)
        local title_bar = frame.add{ type = "flow", direction = "horizontal", name = "title_bar", }
        title_bar.drag_target = frame
        title_bar.add{ type = "label", style = "frame_title", caption = {"game-speed.options-title"}, ignored_by_interaction = true, }
        title_bar.add{ type = "empty-widget", style = "game_speed_title_bar_draggable_space", ignored_by_interaction = true, }
        refs.options_frame_close_button = title_bar.add{ type = "sprite-button", style = "frame_action_button", sprite = "game_speed_settings_icon_white", hovered_sprite = "game_speed_settings_icon_black", clicked_sprite = "game_speed_settings_icon_black", }

        local inside_shallow_frame = frame.add{ type = "frame", style = "inside_shallow_frame", direction = "vertical", }
        inside_shallow_frame.style.top_padding = 6
        inside_shallow_frame.style.bottom_padding = 6
        local bordered_frame = inside_shallow_frame.add{ type = "frame", style = "bordered_frame", direction = "vertical", }
        refs.show_play_until_time_tab_checkbox = bordered_frame.add{ type = "checkbox", style = "caption_checkbox", caption = {"game-speed.show-tab-option", {"game-speed.play-until-time"}}, state = true, }
        refs.show_play_for_limited_time_tab_checkbox = bordered_frame.add{ type = "checkbox", style = "caption_checkbox", caption = {"game-speed.show-tab-option", {"gui-map-editor-time-editor.play-for-limited-time"}}, state = true, }
        refs.show_ticks_checkbox = bordered_frame.add { type = "checkbox", style = "caption_checkbox", caption = {"game-speed.show-ticks-option"}, state = true, }
    end

    -- handle mod settings
    do
        -- set permissions for this player
        update_permissions(player_index)

        -- check mod settings for whether to start with the GUI open
        local open = player.mod_settings["game-speed-gui-open-initially"].value
        main_frame.visible = open
        player.set_shortcut_toggled("game-speed-toggle-gui", open)

        -- set initial position
        local x = player.mod_settings["game-speed-gui-initial-position-x"].value
        local y = player.mod_settings["game-speed-gui-initial-position-y"].value
        main_frame.location = { x, y }

        local show_play_until_time_tab = player.mod_settings["game-speed-show-play-until-time-tab-initially"].value
        local show_play_for_limited_time_tab = player.mod_settings["game-speed-show-play-for-limited-time-tab-initially"].value
        local show_ticks = player.mod_settings["game-speed-show-ticks-initially"].value

        refs.show_play_until_time_tab_checkbox.state = show_play_until_time_tab
        refs.tick_until_flow.visible = show_play_until_time_tab
        refs.show_play_for_limited_time_tab_checkbox.state = show_play_for_limited_time_tab
        refs.tick_for_flow.visible = show_play_for_limited_time_tab
        refs.show_ticks_checkbox.state = show_ticks
        -- update gui based on whether 'show ticks' is enabled
        update_show_ticks(player_index)
    end

    -- actually initialize time label
    update_current_time_label(player_index, game.tick)
end

local function destroy_gui(player_index)
    -- don't actually have to destroy the gui, but do need to remove the references in storage
    storage.player_info[player_index] = nil
end

script.on_init(function()
    -- initialise speed to existing game speed
    storage.current_speed = game.speed

    -- initialise player_info table
    storage.player_info = {}

    -- build gui for all existing players
    for player_index, player in pairs(game.players) do
        build_gui(player_index)
    end
end)

script.on_event(defines.events.on_player_created, function(event)
    build_gui(event.player_index)
end)

script.on_event(defines.events.on_player_removed, function(event)
    destroy_gui(event.player_index)
end)

script.on_load(function()
end)

-- update game speed label and play/pause button based on current game speed and pause state
-- updates for the given player, or for all players if player_index == nil
local function update_gui_internal(player_index)
    -- uses actual game speed not storage.current_speed because actual speed might be different in the case of fast forwarding

    local speed_caption = nil
    if game.speed == 1 then
        speed_caption = {"gui-speed-panel.normal"}
    elseif game.speed < 1 then
        speed_caption = "/ " .. (1 / game.speed)
    else
        speed_caption = "x " .. game.speed
    end
    local caption = game.tick_paused and {"", speed_caption, " ", {"gui-map-editor-time-editor.paused"}} or speed_caption

    local sprite = nil
    local tooltip = nil
    local style = nil
    if game.tick_paused then
        sprite = "utility/play"
        tooltip = {"game-speed.play"}
        -- if there is 1 tick left to run then on_tick will not run until the game is unpaused, so update the sprite now
        style = game.ticks_to_run > 1 and "game_speed_selected_slot_sized_button" or "slot_sized_button"
    else
        sprite = "utility/pause"
        tooltip = {"game-speed.pause"}
        style = "game_speed_selected_slot_sized_button"
    end

    local function do_update(player_index)
        local refs = storage.player_info[player_index].refs

        -- update speed label
        local label = refs.game_speed_label
        label.caption = caption

        -- update play/pause button
        local button = refs.play_pause_button
        button.sprite = sprite
        button.tooltip = tooltip
        button.style = style
    end

    if player_index then
        do_update(player_index)
    else
        for player_index, player_info in pairs(storage.player_info) do
            do_update(player_index)
        end
    end
end

local function update_gui(player_index)
    update_gui_internal(player_index)
end

local function update_gui_for_all_players()
    update_gui_internal()
end

local function toggle_gui(event)
    local player_index = event.player_index
    local refs = storage.player_info[player_index].refs
    local frame = refs.main_frame

    frame.visible = not frame.visible
    frame.bring_to_front()

    -- close options frame
    local options_frame = refs.options_frame
    options_frame.visible = false

    -- toggle shortcut
    local player = game.players[player_index]
    player.set_shortcut_toggled("game-speed-toggle-gui", frame.visible)
end

local function reset_speed()
    storage.current_speed = 1
    game.speed = storage.current_speed
    update_gui_for_all_players()
end

local function speed_down()
    if storage.current_speed > speed_min then
        storage.current_speed = storage.current_speed / 2
    end
    game.speed = storage.current_speed
    update_gui_for_all_players()
end

local function play_pause()
    if game.tick_paused == false and game.ticks_to_run == 0 then
        game.tick_paused = true
    else
        game.tick_paused = false
    end

    -- reset speed in case of fast forward
    game.speed = storage.current_speed

    update_gui_for_all_players()
end

local function speed_up()
    if storage.current_speed < speed_max then
        storage.current_speed = storage.current_speed * 2
    end
    game.speed = storage.current_speed
    update_gui_for_all_players()
end

local function max_speed()
    storage.current_speed = speed_max
    game.speed = storage.current_speed
    update_gui_for_all_players()
end


local function play_for(ticks, is_fast_forward)
    game.tick_paused = true
    game.ticks_to_run = ticks

    if ticks == 0 then
        -- edge case, just do the same thing as if the 'tick until' is about to finish
        game.speed = storage.current_speed
    else
        game.speed = is_fast_forward and fast_forward_speed or storage.current_speed
    end

    update_gui_for_all_players()
end

local function get_play_until_time(player_index)
    local refs = storage.player_info[player_index].refs

    -- get the 'play until' time from the provided textfields
    local hour = tonumber(refs.until_hour_textfield.text, 10) or 0
    local minute = tonumber(refs.until_minute_textfield.text, 10) or 0
    local second = tonumber(refs.until_second_textfield.text, 10) or 0
    local tick = 0
    -- if 'show ticks' is off, then make the tick input 0
    local show_ticks = refs.show_ticks_checkbox.state
    if show_ticks then
        tick = tonumber(refs.until_tick_textfield.text, 10) or 0
    end

    local until_tick = hour * 60 * 60 * 60 + minute * 60 * 60 + second * 60 + tick

    return until_tick, hour, minute, second, tick
end

local function play_until_time(player_index, is_fast_forward)
    local until_tick, hour, minute, second, tick = get_play_until_time(player_index)

    -- reformat the textfields
    local refs = storage.player_info[player_index].refs
    refs.until_hour_textfield.text = string.format("%d", hour)
    refs.until_minute_textfield.text = string.format("%02d", minute)
    refs.until_second_textfield.text = string.format("%02d", second)
    refs.until_tick_textfield.text = string.format("%02d", tick)

    if until_tick < game.tick then
        -- this probably never happens, but just in case?
        return
    end

    play_for(until_tick - game.tick, is_fast_forward)
end

local function update_play_until_buttons_enabled(player_index, current_tick)
    local until_tick = get_play_until_time(player_index)
    local enabled = current_tick < until_tick

    local refs = storage.player_info[player_index].refs
    local play_until_button = refs.play_until_button
    local fast_forward_until_button = refs.fast_forward_until_button

    play_until_button.enabled = enabled
    fast_forward_until_button.enabled = enabled

    if enabled then
        play_until_button.sprite = "game_speed_play_until"
        fast_forward_until_button.sprite = "game_speed_fast_forward_until"
    else
        play_until_button.sprite = "game_speed_play_until_disabled"
        fast_forward_until_button.sprite = "game_speed_fast_forward_until_disabled"
    end
end

local function play_until(event)
    play_until_time(event.player_index, false)
end

local function fast_forward_until(event)
    play_until_time(event.player_index, true)
end

local function tick_custom(event)
    local player_index = event.player_index
    local refs = storage.player_info[player_index].refs
    local ticks = tonumber(refs.tick_for_textfield.text, 10) or 0
    play_for(ticks, false)
end

local function tick_sixty()
    play_for(60, false)
end

local function tick_once()
    play_for(1, false)
end

local function toggle_options(event)
    local player_index = event.player_index
    local refs = storage.player_info[player_index].refs
    local frame = refs.options_frame
    frame.visible = not frame.visible
    frame.bring_to_front()
end

update_show_ticks = function(player_index)
    -- update current time label to show/not show ticks
    update_current_time_label(player_index, game.tick)

    local refs = storage.player_info[player_index].refs
    local show_ticks = refs.show_ticks_checkbox.state
    for _, element in pairs(refs.until_tick_input_elements) do
        element.visible = show_ticks
    end
    -- update 'play until' buttons, because hiding the tick input makes it implicitly 0
    update_play_until_buttons_enabled(player_index, game.tick)
end

update_permissions = function(player_index)
    local player = game.players[player_index]
    local permitted = true
    local refs = storage.player_info[player_index].refs
    refs.game_speed_controls_flow.visible = permitted
    refs.tick_until_controls_flow.visible = permitted
    refs.tick_for_controls_flow.visible = permitted
end

script.on_event(defines.events.on_gui_click, function(event)
    local player_index = event.player_index
    local refs = storage.player_info[player_index].refs
    local handlers = {
        [refs.reset_speed_button] = reset_speed,
        [refs.speed_down_button] = speed_down,
        [refs.play_pause_button] = play_pause,
        [refs.speed_up_button] = speed_up,
        [refs.max_speed_button] = max_speed,
        [refs.play_until_button] = play_until,
        [refs.fast_forward_until_button] = fast_forward_until,
        [refs.tick_custom_button] = tick_custom,
        [refs.tick_sixty_button] = tick_sixty,
        [refs.tick_once_button] = tick_once,
        [refs.toggle_options_button] = toggle_options,
        [refs.main_frame_close_button] = toggle_gui,
        [refs.options_frame_close_button] = toggle_options,
    }
    for element, handler in pairs(handlers) do
        if event.element == element then
            handler(event)
        end
    end
end)

script.on_event(defines.events.on_gui_checked_state_changed, function(event)
    local player_index = event.player_index
    local refs = storage.player_info[player_index].refs
    if event.element == refs.show_play_until_time_tab_checkbox then
        local flow = refs.tick_until_flow
        flow.visible = event.element.state
    elseif event.element == refs.show_play_for_limited_time_tab_checkbox then
        local flow = refs.tick_for_flow
        flow.visible = event.element.state
    elseif event.element == refs.show_ticks_checkbox then
        update_show_ticks(player_index)
    end
end)

script.on_event(defines.events.on_gui_text_changed, function(event)
    local player_index = event.player_index
    local refs = storage.player_info[player_index].refs
    local element = event.element
    if element == refs.until_hour_textfield or
        element == refs.until_minute_textfield or
        element == refs.until_second_textfield or
        element == refs.until_tick_textfield then
        update_play_until_buttons_enabled(player_index, game.tick)
    end
end)

script.on_event(defines.events.on_tick, function()
    -- once this function has run, tick number game.tick will be finished and it will be
    -- tick number game.tick + 1
    update_current_time_label_for_all_players(game.tick + 1)

    for player_index, player_info in pairs(storage.player_info) do
        -- update whether the 'play until' buttons are enabled, also with game.tick + 1
        update_play_until_buttons_enabled(player_index, game.tick + 1)
    end

    if game.speed ~= storage.current_speed and game.speed ~= fast_forward_speed then
        -- the speed has been changed outside of the mod, so update the mod state to match
        storage.current_speed = game.speed
    end

    if game.speed == fast_forward_speed and game.ticks_to_run == 1 then
        -- 'tick until' is about the finish, so reset the game speed to normal
        game.speed = storage.current_speed
    end

    -- update the gui in case the game speed or play/pause state was changed by something other than the mod
    update_gui_for_all_players()
end)

script.on_event(defines.events.on_player_toggled_map_editor, function()
    -- toggling the map editor can change the play/pause state so update the gui
    update_gui_for_all_players()
end)

script.on_event(defines.events.on_runtime_mod_setting_changed, function(event)
    if event.setting == "game-speed-only-allow-admins" then
        -- update permissions for all players
        for player_index, player_info in pairs(storage.player_info) do
            update_permissions(player_index)
        end
    elseif event.setting == "game-speed-gui-initial-position-x" or
        event.setting == "game-speed-gui-initial-position-y" then
        -- allow changing the GUI position with the mod settings in case the player accidentally positions the GUI outside their screen
        local player_index = event.player_index
        local player = game.players[player_index]
        local refs = storage.player_info[player_index].refs
        local main_frame = refs.main_frame
        local x = player.mod_settings["game-speed-gui-initial-position-x"].value
        local y = player.mod_settings["game-speed-gui-initial-position-y"].value
        main_frame.location = { x, y }
    end
end)

script.on_event(defines.events.on_player_promoted, function(event)
    local player_index = event.player_index

    -- handle weird double promotion when game starts
    if storage.player_info[player_index] == nil then
        return
    end

    update_permissions(player_index)
end)

script.on_event(defines.events.on_player_demoted, function(event)
    local player_index = event.player_index
    update_permissions(player_index)
end)

local function add_admin_check(handler)
    return function(event)
        handler(event)
    end
end

script.on_event("game-speed-toggle-gui", toggle_gui)
script.on_event("game-speed-reset-speed", add_admin_check(reset_speed))
script.on_event("game-speed-speed-down", add_admin_check(speed_down))
script.on_event("game-speed-play-pause", add_admin_check(play_pause))
script.on_event("game-speed-speed-up", add_admin_check(speed_up))
script.on_event("game-speed-max-speed", add_admin_check(max_speed))
script.on_event("game-speed-tick-custom", add_admin_check(tick_custom))
script.on_event("game-speed-tick-sixty", add_admin_check(tick_sixty))
script.on_event("game-speed-tick-once", add_admin_check(tick_once))

script.on_event(defines.events.on_lua_shortcut, function(event)
    if event.prototype_name == "game-speed-toggle-gui" then
        toggle_gui(event)
    end
end)

