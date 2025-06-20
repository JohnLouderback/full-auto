import {showModLauncher} from "@library/Tasks"
import {forever} from "@library/Utils"

await showModLauncher({
  game: {
    id: "doom",
    displayName: "DOOM",
    logoPath: "H:\\Downloads\\Doom-Logo-PNG-Isolated-File.png",
    description: "Doom (1993) is a landmark first-person shooter by id Software. Players battle demons on Mars' moons after a failed experiment. Its fast gameplay and mod support made it a cultural phenomenon. Doom also helped popularize multiplayer and 3D graphics in games.",
    gamePath: "C:/Doom",
    mods: [
      {
        id: 'doom-64',
        displayName: 'Doom 64',
        description: "Doom 64 (1997) is a sequel to the original Doom series, developed exclusively for the Nintendo 64. It features new levels, graphics, and atmospheric lighting. The gameplay retains classic Doom mechanics with a darker, more horror-focused tone. Doom 64 gained cult status and was later re-released on modern platforms."
      },
      {
        id: 'my-house',
        displayName: 'My House',
        description: "My House (2023) is a custom Doom II WAD by modder vaatik. It begins as a tour of a suburban home before expanding into unexpected territory. The level features intricate design, atmospheric tension, and subtle narrative elements. Widely praised, it stands out as one of the most ambitious WADs in recent years."
      }
    ]
  },
  backgroundImagePath: "H:\\Downloads\\JDUI_UD_P.png"
});

await forever();