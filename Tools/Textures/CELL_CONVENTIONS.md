# Atlas cell/tile conventions (builders <-> texture generator)
Atlas layout/tags: `Tools/Blender/ll_blender/atlas.py` (do not edit). v measured from the BOTTOM of the image (PNG row 0 = top).
* 'tile' atlases (ENV_*_2K): each 512px tile is one seamlessly tileable material (wraps in both axes). 1 tile repeat = `mpt` metres.
* 'cell' atlases: 256px (HERO_*_1K, mpt .5/.8/.6) / 128px (PROP_*_512, mpt .25) cells. Builders map faces by planar projection starting at the
  cell's bottom-left (3% inset) so **material cells must look uniform/isotropic (no big unique features, no strong gradients, no border frame)**.
* PAINTED cells (builders map the sub-rect they want of the whole cell with explicit UVs - motif fills the cell, 6% margin):
  - faces `face_walker face_runner face_spitter face_goliath face_aras face_selin face_kemal face_elif`: FRONT PORTRAIT filling the
    cell: forehead/hair band v>0.78, eye line v=0.58 (eyes centred u=0.30 and u=0.70, ~0.12 wide), nose v~0.42 (u .5), mouth v=0.25 (centre u=.5,
    ~0.34 wide), chin v<0.12; skin colour edge-to-edge (so cell borders blend with the skin cell). Zombie faces: sunken dark eyes (walker milky/white
    pupils, runner wide bloodshot red, spitter green glowing eyes + open dripping mouth, goliath tiny yellow eyes + huge jaw), stitches/wounds. Survivors:
    aras tan skin dark short hair top, selin light skin + dark hair/glasses-less, kemal dark skin + grey stubble beard, elif light skin + brown hair.
  - prop labels `can_tomato can_beans can_tuna bottle_water bottle_soda book_red book_blue book_green wrapper_snack box_cardboard pill_bottle matchbox
    cigarette_pack photo_frame mug_ceramic toy_plastic` : full wrap-around label art (horizontal strips/text-like blocks, brand colour fields, a central
    emblem: tomato / beans / fish; water drop; book title bands with spine lines). Must stay readable when squeezed onto a cylinder (u wraps 0..1).
  - `outlet_white` (white plate, two dark sockets centred), `paper_note`/`document_paper` (cream paper, ruled text-like lines, stamp), `keycard` (card face: magnetic
    stripe v .72-.88, gold chip u.15-.32 v.40-.62, white text lines, colour band), `screen_green` (dark green CRT with bright scanlines and UI blocks),
    `circuit_green` (green PCB with traces/pads), `medkit_white` (white with red cross centred), `radio_small` (speaker grille + dial), `bandage`, `bandage_roll`,
    `syringe`, `glass_vial`, `bulb_glass`, `coin_gold`, `key_brass`, `battery_metal/battery_green` (label strip + stripe), `tape_duct` (grey duct tape sheen), `lighter`.
  - everything else = plain material cells.
