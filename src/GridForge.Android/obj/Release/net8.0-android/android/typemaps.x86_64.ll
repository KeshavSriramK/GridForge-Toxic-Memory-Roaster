; ModuleID = 'typemaps.x86_64.ll'
source_filename = "typemaps.x86_64.ll"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-android21"

%struct.TypeMapJava = type {
	i32, ; uint32_t module_index
	i32, ; uint32_t type_token_id
	i32 ; uint32_t java_name_index
}

%struct.TypeMapModule = type {
	[16 x i8], ; uint8_t module_uuid[16]
	i32, ; uint32_t entry_count
	i32, ; uint32_t duplicate_count
	ptr, ; TypeMapModuleEntry map
	ptr, ; TypeMapModuleEntry duplicate_map
	ptr, ; char* assembly_name
	ptr, ; MonoImage image
	i32, ; uint32_t java_name_width
	ptr ; uint8_t java_map
}

%struct.TypeMapModuleEntry = type {
	i32, ; uint32_t type_token_id
	i32 ; uint32_t java_map_index
}

@map_module_count = dso_local local_unnamed_addr constant i32 2, align 4

@java_type_count = dso_local local_unnamed_addr constant i32 42, align 4

; Managed modules map
@map_modules = dso_local local_unnamed_addr global [2 x %struct.TypeMapModule] [
	%struct.TypeMapModule {
		[16 x i8] c"\8A\04T\D1\E3\B9\E4N\9D4\9Bg7\91~\96", ; module_uuid: d154048a-b9e3-4ee4-9d34-9b6737917e96
		i32 41, ; uint32_t entry_count (0x29)
		i32 10, ; uint32_t duplicate_count (0xa)
		ptr @module0_managed_to_java, ; TypeMapModuleEntry* map
		ptr @module0_managed_to_java_duplicates, ; TypeMapModuleEntry* duplicate_map
		ptr @.TypeMapModule.0_assembly_name, ; assembly_name: Mono.Android
		ptr null, ; MonoImage* image
		i32 0, ; uint32_t java_name_width (0x0)
		ptr null; uint8_t* java_map (0x0)
	}, ; 0
	%struct.TypeMapModule {
		[16 x i8] c"\91\C4T\DB\9F\B5rA\85\BD^\E9\F5H\94\CB", ; module_uuid: db54c491-b59f-4172-85bd-5ee9f54894cb
		i32 1, ; uint32_t entry_count (0x1)
		i32 0, ; uint32_t duplicate_count (0x0)
		ptr @module1_managed_to_java, ; TypeMapModuleEntry* map
		ptr null, ; TypeMapModuleEntry* duplicate_map
		ptr @.TypeMapModule.1_assembly_name, ; assembly_name: GridForge.Android
		ptr null, ; MonoImage* image
		i32 0, ; uint32_t java_name_width (0x0)
		ptr null; uint8_t* java_map (0x0)
	} ; 1
], align 16

; Java types name hashes
@map_java_hashes = dso_local local_unnamed_addr constant [42 x i64] [
	i64 128182020419974451, ; 0: 0x1c764de51b97533 => java/lang/String
	i64 361870449891484378, ; 1: 0x5059f41c47e22da => android/os/Bundle
	i64 363417747702605178, ; 2: 0x50b1e841ce2e57a => android/widget/TextView
	i64 870874870088288028, ; 3: 0xc15f8148b6d471c => java/lang/Exception
	i64 1317579852464953526, ; 4: 0x1248fbe51d6298b6 => java/io/FileInputStream
	i64 1320822650197077237, ; 5: 0x12548133cc496cf5 => android/runtime/JavaProxyThrowable
	i64 1747499027921055994, ; 6: 0x18405d1b749330fa => android/os/BaseBundle
	i64 1831728799718484971, ; 7: 0x196b9ba37012abeb => java/io/IOException
	i64 2164140653916027403, ; 8: 0x1e08927568a57a0b => java/io/InputStream
	i64 3476617847597562063, ; 9: 0x303f6d8331d5f8cf => java/io/PrintWriter
	i64 4305371449952891808, ; 10: 0x3bbfc085dc8cf3a0 => java/lang/Class
	i64 4337602266058890002, ; 11: 0x3c324247855e9b12 => crc64b79b9ba72943b64e/MainActivity
	i64 5214467817578676657, ; 12: 0x485d82da477bc1b1 => java/lang/Error
	i64 6000768439507874839, ; 13: 0x5347031a303df417 => java/lang/Enum
	i64 6116679261601087867, ; 14: 0x54e2cf6180bb417b => android/widget/LinearLayout
	i64 8190305621607579207, ; 15: 0x71a9cf9199cdfe47 => java/nio/channels/spi/AbstractInterruptibleChannel
	i64 8487642170263250902, ; 16: 0x75ca29959b2aa7d6 => android/content/ContextWrapper
	i64 8587172038193766563, ; 17: 0x772bc378d1b4e0a3 => java/lang/Runnable
	i64 9261475547774197607, ; 18: 0x80875ef0f90c1f67 => android/widget/ImageView
	i64 9667515047141612341, ; 19: 0x8629e9b6f59e9b35 => java/lang/Thread
	i64 9869939015140501507, ; 20: 0x88f9113db837e803 => android/app/Activity
	i64 11112718717483603117, ; 21: 0x9a384ecbbc71d4ad => android/os/Handler
	i64 11573301743732151818, ; 22: 0xa09ca09e3190560a => mono/java/lang/RunnableImplementor
	i64 11954228872253987625, ; 23: 0xa5e5f3d2b66adb29 => android/view/View
	i64 12228984007958404582, ; 24: 0xa9b61429ce4b1de6 => android/content/Context
	i64 12476375190645835422, ; 25: 0xad24fd221af1069e => android/os/Looper
	i64 13402779434266666368, ; 26: 0xba003ce26e602580 => mono/android/TypeManager
	i64 13770727111868296170, ; 27: 0xbf1b735909c02bea => java/io/StringWriter
	i64 13805562342397192842, ; 28: 0xbf9735ce2f182a8a => android/util/AttributeSet
	i64 14031640676547298208, ; 29: 0xc2ba66da3d8603a0 => java/nio/channels/FileChannel
	i64 14167891754637755728, ; 30: 0xc49e767c735e8550 => java/lang/Object
	i64 14206023932851353817, ; 31: 0xc525ef800c4d78d9 => mono/android/runtime/OutputStreamAdapter
	i64 14348802445188264590, ; 32: 0xc7212fccf9b3f28e => android/widget/ImageView$ScaleType
	i64 14940408132235664607, ; 33: 0xcf56fe09e1439cdf => java/lang/Throwable
	i64 15142650489578038267, ; 34: 0xd22580641d31a3fb => java/lang/StackTraceElement
	i64 15633873768898914415, ; 35: 0xd8f6ad5c6a84686f => java/io/Writer
	i64 16314168557433322311, ; 36: 0xe26791dde7a8fb47 => android/view/ContextThemeWrapper
	i64 16542847110558016359, ; 37: 0xe593ffcc9e686367 => android/app/Application
	i64 16723123314454325679, ; 38: 0xe814780d351a69af => mono/android/runtime/InputStreamAdapter
	i64 16832017439803262409, ; 39: 0xe99756ae80a745c9 => android/view/ViewGroup
	i64 17125416866214736517, ; 40: 0xeda9b3e7cd367285 => java/io/OutputStream
	i64 17356379843024169959 ; 41: 0xf0de3f825a34abe7 => android/graphics/Color
], align 16

@module0_managed_to_java = internal dso_local constant [41 x %struct.TypeMapModuleEntry] [
	%struct.TypeMapModuleEntry {
		i32 33554488, ; uint32_t type_token_id (0x2000038)
		i32 18; uint32_t java_map_index (0x12)
	}, ; 0
	%struct.TypeMapModuleEntry {
		i32 33554489, ; uint32_t type_token_id (0x2000039)
		i32 32; uint32_t java_map_index (0x20)
	}, ; 1
	%struct.TypeMapModuleEntry {
		i32 33554490, ; uint32_t type_token_id (0x200003a)
		i32 14; uint32_t java_map_index (0xe)
	}, ; 2
	%struct.TypeMapModuleEntry {
		i32 33554491, ; uint32_t type_token_id (0x200003b)
		i32 2; uint32_t java_map_index (0x2)
	}, ; 3
	%struct.TypeMapModuleEntry {
		i32 33554493, ; uint32_t type_token_id (0x200003d)
		i32 28; uint32_t java_map_index (0x1c)
	}, ; 4
	%struct.TypeMapModuleEntry {
		i32 33554495, ; uint32_t type_token_id (0x200003f)
		i32 6; uint32_t java_map_index (0x6)
	}, ; 5
	%struct.TypeMapModuleEntry {
		i32 33554496, ; uint32_t type_token_id (0x2000040)
		i32 1; uint32_t java_map_index (0x1)
	}, ; 6
	%struct.TypeMapModuleEntry {
		i32 33554497, ; uint32_t type_token_id (0x2000041)
		i32 21; uint32_t java_map_index (0x15)
	}, ; 7
	%struct.TypeMapModuleEntry {
		i32 33554498, ; uint32_t type_token_id (0x2000042)
		i32 25; uint32_t java_map_index (0x19)
	}, ; 8
	%struct.TypeMapModuleEntry {
		i32 33554500, ; uint32_t type_token_id (0x2000044)
		i32 20; uint32_t java_map_index (0x14)
	}, ; 9
	%struct.TypeMapModuleEntry {
		i32 33554501, ; uint32_t type_token_id (0x2000045)
		i32 37; uint32_t java_map_index (0x25)
	}, ; 10
	%struct.TypeMapModuleEntry {
		i32 33554504, ; uint32_t type_token_id (0x2000048)
		i32 36; uint32_t java_map_index (0x24)
	}, ; 11
	%struct.TypeMapModuleEntry {
		i32 33554505, ; uint32_t type_token_id (0x2000049)
		i32 23; uint32_t java_map_index (0x17)
	}, ; 12
	%struct.TypeMapModuleEntry {
		i32 33554506, ; uint32_t type_token_id (0x200004a)
		i32 39; uint32_t java_map_index (0x27)
	}, ; 13
	%struct.TypeMapModuleEntry {
		i32 33554527, ; uint32_t type_token_id (0x200005f)
		i32 38; uint32_t java_map_index (0x26)
	}, ; 14
	%struct.TypeMapModuleEntry {
		i32 33554529, ; uint32_t type_token_id (0x2000061)
		i32 5; uint32_t java_map_index (0x5)
	}, ; 15
	%struct.TypeMapModuleEntry {
		i32 33554540, ; uint32_t type_token_id (0x200006c)
		i32 31; uint32_t java_map_index (0x1f)
	}, ; 16
	%struct.TypeMapModuleEntry {
		i32 33554546, ; uint32_t type_token_id (0x2000072)
		i32 41; uint32_t java_map_index (0x29)
	}, ; 17
	%struct.TypeMapModuleEntry {
		i32 33554549, ; uint32_t type_token_id (0x2000075)
		i32 24; uint32_t java_map_index (0x18)
	}, ; 18
	%struct.TypeMapModuleEntry {
		i32 33554551, ; uint32_t type_token_id (0x2000077)
		i32 16; uint32_t java_map_index (0x10)
	}, ; 19
	%struct.TypeMapModuleEntry {
		i32 33554553, ; uint32_t type_token_id (0x2000079)
		i32 29; uint32_t java_map_index (0x1d)
	}, ; 20
	%struct.TypeMapModuleEntry {
		i32 33554555, ; uint32_t type_token_id (0x200007b)
		i32 15; uint32_t java_map_index (0xf)
	}, ; 21
	%struct.TypeMapModuleEntry {
		i32 33554557, ; uint32_t type_token_id (0x200007d)
		i32 4; uint32_t java_map_index (0x4)
	}, ; 22
	%struct.TypeMapModuleEntry {
		i32 33554558, ; uint32_t type_token_id (0x200007e)
		i32 8; uint32_t java_map_index (0x8)
	}, ; 23
	%struct.TypeMapModuleEntry {
		i32 33554560, ; uint32_t type_token_id (0x2000080)
		i32 7; uint32_t java_map_index (0x7)
	}, ; 24
	%struct.TypeMapModuleEntry {
		i32 33554561, ; uint32_t type_token_id (0x2000081)
		i32 40; uint32_t java_map_index (0x28)
	}, ; 25
	%struct.TypeMapModuleEntry {
		i32 33554563, ; uint32_t type_token_id (0x2000083)
		i32 9; uint32_t java_map_index (0x9)
	}, ; 26
	%struct.TypeMapModuleEntry {
		i32 33554564, ; uint32_t type_token_id (0x2000084)
		i32 27; uint32_t java_map_index (0x1b)
	}, ; 27
	%struct.TypeMapModuleEntry {
		i32 33554565, ; uint32_t type_token_id (0x2000085)
		i32 35; uint32_t java_map_index (0x23)
	}, ; 28
	%struct.TypeMapModuleEntry {
		i32 33554567, ; uint32_t type_token_id (0x2000087)
		i32 10; uint32_t java_map_index (0xa)
	}, ; 29
	%struct.TypeMapModuleEntry {
		i32 33554568, ; uint32_t type_token_id (0x2000088)
		i32 13; uint32_t java_map_index (0xd)
	}, ; 30
	%struct.TypeMapModuleEntry {
		i32 33554570, ; uint32_t type_token_id (0x200008a)
		i32 12; uint32_t java_map_index (0xc)
	}, ; 31
	%struct.TypeMapModuleEntry {
		i32 33554571, ; uint32_t type_token_id (0x200008b)
		i32 3; uint32_t java_map_index (0x3)
	}, ; 32
	%struct.TypeMapModuleEntry {
		i32 33554572, ; uint32_t type_token_id (0x200008c)
		i32 17; uint32_t java_map_index (0x11)
	}, ; 33
	%struct.TypeMapModuleEntry {
		i32 33554574, ; uint32_t type_token_id (0x200008e)
		i32 30; uint32_t java_map_index (0x1e)
	}, ; 34
	%struct.TypeMapModuleEntry {
		i32 33554575, ; uint32_t type_token_id (0x200008f)
		i32 34; uint32_t java_map_index (0x22)
	}, ; 35
	%struct.TypeMapModuleEntry {
		i32 33554576, ; uint32_t type_token_id (0x2000090)
		i32 0; uint32_t java_map_index (0x0)
	}, ; 36
	%struct.TypeMapModuleEntry {
		i32 33554578, ; uint32_t type_token_id (0x2000092)
		i32 19; uint32_t java_map_index (0x13)
	}, ; 37
	%struct.TypeMapModuleEntry {
		i32 33554579, ; uint32_t type_token_id (0x2000093)
		i32 22; uint32_t java_map_index (0x16)
	}, ; 38
	%struct.TypeMapModuleEntry {
		i32 33554580, ; uint32_t type_token_id (0x2000094)
		i32 33; uint32_t java_map_index (0x21)
	}, ; 39
	%struct.TypeMapModuleEntry {
		i32 33554591, ; uint32_t type_token_id (0x200009f)
		i32 26; uint32_t java_map_index (0x1a)
	} ; 40
], align 16

@module0_managed_to_java_duplicates = internal dso_local constant [10 x %struct.TypeMapModuleEntry] [
	%struct.TypeMapModuleEntry {
		i32 33554494, ; uint32_t type_token_id (0x200003e)
		i32 28; uint32_t java_map_index (0x1c)
	}, ; 0
	%struct.TypeMapModuleEntry {
		i32 33554507, ; uint32_t type_token_id (0x200004b)
		i32 39; uint32_t java_map_index (0x27)
	}, ; 1
	%struct.TypeMapModuleEntry {
		i32 33554550, ; uint32_t type_token_id (0x2000076)
		i32 24; uint32_t java_map_index (0x18)
	}, ; 2
	%struct.TypeMapModuleEntry {
		i32 33554554, ; uint32_t type_token_id (0x200007a)
		i32 29; uint32_t java_map_index (0x1d)
	}, ; 3
	%struct.TypeMapModuleEntry {
		i32 33554556, ; uint32_t type_token_id (0x200007c)
		i32 15; uint32_t java_map_index (0xf)
	}, ; 4
	%struct.TypeMapModuleEntry {
		i32 33554559, ; uint32_t type_token_id (0x200007f)
		i32 8; uint32_t java_map_index (0x8)
	}, ; 5
	%struct.TypeMapModuleEntry {
		i32 33554562, ; uint32_t type_token_id (0x2000082)
		i32 40; uint32_t java_map_index (0x28)
	}, ; 6
	%struct.TypeMapModuleEntry {
		i32 33554566, ; uint32_t type_token_id (0x2000086)
		i32 35; uint32_t java_map_index (0x23)
	}, ; 7
	%struct.TypeMapModuleEntry {
		i32 33554569, ; uint32_t type_token_id (0x2000089)
		i32 13; uint32_t java_map_index (0xd)
	}, ; 8
	%struct.TypeMapModuleEntry {
		i32 33554573, ; uint32_t type_token_id (0x200008d)
		i32 17; uint32_t java_map_index (0x11)
	} ; 9
], align 16

@module1_managed_to_java = internal dso_local constant [1 x %struct.TypeMapModuleEntry] [
	%struct.TypeMapModuleEntry {
		i32 33554434, ; uint32_t type_token_id (0x2000002)
		i32 11; uint32_t java_map_index (0xb)
	} ; 0
], align 4

; Java to managed map
@map_java = dso_local local_unnamed_addr constant [42 x %struct.TypeMapJava] [
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554576, ; uint32_t type_token_id (0x2000090)
		i32 36; uint32_t java_name_index (0x24)
	}, ; 0
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554496, ; uint32_t type_token_id (0x2000040)
		i32 6; uint32_t java_name_index (0x6)
	}, ; 1
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554491, ; uint32_t type_token_id (0x200003b)
		i32 3; uint32_t java_name_index (0x3)
	}, ; 2
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554571, ; uint32_t type_token_id (0x200008b)
		i32 32; uint32_t java_name_index (0x20)
	}, ; 3
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554557, ; uint32_t type_token_id (0x200007d)
		i32 22; uint32_t java_name_index (0x16)
	}, ; 4
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554529, ; uint32_t type_token_id (0x2000061)
		i32 15; uint32_t java_name_index (0xf)
	}, ; 5
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554495, ; uint32_t type_token_id (0x200003f)
		i32 5; uint32_t java_name_index (0x5)
	}, ; 6
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554560, ; uint32_t type_token_id (0x2000080)
		i32 24; uint32_t java_name_index (0x18)
	}, ; 7
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554558, ; uint32_t type_token_id (0x200007e)
		i32 23; uint32_t java_name_index (0x17)
	}, ; 8
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554563, ; uint32_t type_token_id (0x2000083)
		i32 26; uint32_t java_name_index (0x1a)
	}, ; 9
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554567, ; uint32_t type_token_id (0x2000087)
		i32 29; uint32_t java_name_index (0x1d)
	}, ; 10
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554434, ; uint32_t type_token_id (0x2000002)
		i32 41; uint32_t java_name_index (0x29)
	}, ; 11
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554570, ; uint32_t type_token_id (0x200008a)
		i32 31; uint32_t java_name_index (0x1f)
	}, ; 12
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554568, ; uint32_t type_token_id (0x2000088)
		i32 30; uint32_t java_name_index (0x1e)
	}, ; 13
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554490, ; uint32_t type_token_id (0x200003a)
		i32 2; uint32_t java_name_index (0x2)
	}, ; 14
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554555, ; uint32_t type_token_id (0x200007b)
		i32 21; uint32_t java_name_index (0x15)
	}, ; 15
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554551, ; uint32_t type_token_id (0x2000077)
		i32 19; uint32_t java_name_index (0x13)
	}, ; 16
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 0, ; uint32_t type_token_id (0x0)
		i32 33; uint32_t java_name_index (0x21)
	}, ; 17
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554488, ; uint32_t type_token_id (0x2000038)
		i32 0; uint32_t java_name_index (0x0)
	}, ; 18
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554578, ; uint32_t type_token_id (0x2000092)
		i32 37; uint32_t java_name_index (0x25)
	}, ; 19
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554500, ; uint32_t type_token_id (0x2000044)
		i32 9; uint32_t java_name_index (0x9)
	}, ; 20
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554497, ; uint32_t type_token_id (0x2000041)
		i32 7; uint32_t java_name_index (0x7)
	}, ; 21
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554579, ; uint32_t type_token_id (0x2000093)
		i32 38; uint32_t java_name_index (0x26)
	}, ; 22
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554505, ; uint32_t type_token_id (0x2000049)
		i32 12; uint32_t java_name_index (0xc)
	}, ; 23
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554549, ; uint32_t type_token_id (0x2000075)
		i32 18; uint32_t java_name_index (0x12)
	}, ; 24
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554498, ; uint32_t type_token_id (0x2000042)
		i32 8; uint32_t java_name_index (0x8)
	}, ; 25
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554591, ; uint32_t type_token_id (0x200009f)
		i32 40; uint32_t java_name_index (0x28)
	}, ; 26
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554564, ; uint32_t type_token_id (0x2000084)
		i32 27; uint32_t java_name_index (0x1b)
	}, ; 27
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 0, ; uint32_t type_token_id (0x0)
		i32 4; uint32_t java_name_index (0x4)
	}, ; 28
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554553, ; uint32_t type_token_id (0x2000079)
		i32 20; uint32_t java_name_index (0x14)
	}, ; 29
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554574, ; uint32_t type_token_id (0x200008e)
		i32 34; uint32_t java_name_index (0x22)
	}, ; 30
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554540, ; uint32_t type_token_id (0x200006c)
		i32 16; uint32_t java_name_index (0x10)
	}, ; 31
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554489, ; uint32_t type_token_id (0x2000039)
		i32 1; uint32_t java_name_index (0x1)
	}, ; 32
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554580, ; uint32_t type_token_id (0x2000094)
		i32 39; uint32_t java_name_index (0x27)
	}, ; 33
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554575, ; uint32_t type_token_id (0x200008f)
		i32 35; uint32_t java_name_index (0x23)
	}, ; 34
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554565, ; uint32_t type_token_id (0x2000085)
		i32 28; uint32_t java_name_index (0x1c)
	}, ; 35
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554504, ; uint32_t type_token_id (0x2000048)
		i32 11; uint32_t java_name_index (0xb)
	}, ; 36
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554501, ; uint32_t type_token_id (0x2000045)
		i32 10; uint32_t java_name_index (0xa)
	}, ; 37
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554527, ; uint32_t type_token_id (0x200005f)
		i32 14; uint32_t java_name_index (0xe)
	}, ; 38
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554506, ; uint32_t type_token_id (0x200004a)
		i32 13; uint32_t java_name_index (0xd)
	}, ; 39
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554561, ; uint32_t type_token_id (0x2000081)
		i32 25; uint32_t java_name_index (0x19)
	}, ; 40
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554546, ; uint32_t type_token_id (0x2000072)
		i32 17; uint32_t java_name_index (0x11)
	} ; 41
], align 16

; Java type names
@java_type_names = dso_local local_unnamed_addr constant [42 x ptr] [
	ptr @.str.0, ; 0
	ptr @.str.1, ; 1
	ptr @.str.2, ; 2
	ptr @.str.3, ; 3
	ptr @.str.4, ; 4
	ptr @.str.5, ; 5
	ptr @.str.6, ; 6
	ptr @.str.7, ; 7
	ptr @.str.8, ; 8
	ptr @.str.9, ; 9
	ptr @.str.10, ; 10
	ptr @.str.11, ; 11
	ptr @.str.12, ; 12
	ptr @.str.13, ; 13
	ptr @.str.14, ; 14
	ptr @.str.15, ; 15
	ptr @.str.16, ; 16
	ptr @.str.17, ; 17
	ptr @.str.18, ; 18
	ptr @.str.19, ; 19
	ptr @.str.20, ; 20
	ptr @.str.21, ; 21
	ptr @.str.22, ; 22
	ptr @.str.23, ; 23
	ptr @.str.24, ; 24
	ptr @.str.25, ; 25
	ptr @.str.26, ; 26
	ptr @.str.27, ; 27
	ptr @.str.28, ; 28
	ptr @.str.29, ; 29
	ptr @.str.30, ; 30
	ptr @.str.31, ; 31
	ptr @.str.32, ; 32
	ptr @.str.33, ; 33
	ptr @.str.34, ; 34
	ptr @.str.35, ; 35
	ptr @.str.36, ; 36
	ptr @.str.37, ; 37
	ptr @.str.38, ; 38
	ptr @.str.39, ; 39
	ptr @.str.40, ; 40
	ptr @.str.41 ; 41
], align 16

; Strings
@.str.0 = private unnamed_addr constant [25 x i8] c"android/widget/ImageView\00", align 16
@.str.1 = private unnamed_addr constant [35 x i8] c"android/widget/ImageView$ScaleType\00", align 16
@.str.2 = private unnamed_addr constant [28 x i8] c"android/widget/LinearLayout\00", align 16
@.str.3 = private unnamed_addr constant [24 x i8] c"android/widget/TextView\00", align 16
@.str.4 = private unnamed_addr constant [26 x i8] c"android/util/AttributeSet\00", align 16
@.str.5 = private unnamed_addr constant [22 x i8] c"android/os/BaseBundle\00", align 16
@.str.6 = private unnamed_addr constant [18 x i8] c"android/os/Bundle\00", align 16
@.str.7 = private unnamed_addr constant [19 x i8] c"android/os/Handler\00", align 16
@.str.8 = private unnamed_addr constant [18 x i8] c"android/os/Looper\00", align 16
@.str.9 = private unnamed_addr constant [21 x i8] c"android/app/Activity\00", align 16
@.str.10 = private unnamed_addr constant [24 x i8] c"android/app/Application\00", align 16
@.str.11 = private unnamed_addr constant [33 x i8] c"android/view/ContextThemeWrapper\00", align 16
@.str.12 = private unnamed_addr constant [18 x i8] c"android/view/View\00", align 16
@.str.13 = private unnamed_addr constant [23 x i8] c"android/view/ViewGroup\00", align 16
@.str.14 = private unnamed_addr constant [40 x i8] c"mono/android/runtime/InputStreamAdapter\00", align 16
@.str.15 = private unnamed_addr constant [35 x i8] c"android/runtime/JavaProxyThrowable\00", align 16
@.str.16 = private unnamed_addr constant [41 x i8] c"mono/android/runtime/OutputStreamAdapter\00", align 16
@.str.17 = private unnamed_addr constant [23 x i8] c"android/graphics/Color\00", align 16
@.str.18 = private unnamed_addr constant [24 x i8] c"android/content/Context\00", align 16
@.str.19 = private unnamed_addr constant [31 x i8] c"android/content/ContextWrapper\00", align 16
@.str.20 = private unnamed_addr constant [30 x i8] c"java/nio/channels/FileChannel\00", align 16
@.str.21 = private unnamed_addr constant [51 x i8] c"java/nio/channels/spi/AbstractInterruptibleChannel\00", align 16
@.str.22 = private unnamed_addr constant [24 x i8] c"java/io/FileInputStream\00", align 16
@.str.23 = private unnamed_addr constant [20 x i8] c"java/io/InputStream\00", align 16
@.str.24 = private unnamed_addr constant [20 x i8] c"java/io/IOException\00", align 16
@.str.25 = private unnamed_addr constant [21 x i8] c"java/io/OutputStream\00", align 16
@.str.26 = private unnamed_addr constant [20 x i8] c"java/io/PrintWriter\00", align 16
@.str.27 = private unnamed_addr constant [21 x i8] c"java/io/StringWriter\00", align 16
@.str.28 = private unnamed_addr constant [15 x i8] c"java/io/Writer\00", align 1
@.str.29 = private unnamed_addr constant [16 x i8] c"java/lang/Class\00", align 16
@.str.30 = private unnamed_addr constant [15 x i8] c"java/lang/Enum\00", align 1
@.str.31 = private unnamed_addr constant [16 x i8] c"java/lang/Error\00", align 16
@.str.32 = private unnamed_addr constant [20 x i8] c"java/lang/Exception\00", align 16
@.str.33 = private unnamed_addr constant [19 x i8] c"java/lang/Runnable\00", align 16
@.str.34 = private unnamed_addr constant [17 x i8] c"java/lang/Object\00", align 16
@.str.35 = private unnamed_addr constant [28 x i8] c"java/lang/StackTraceElement\00", align 16
@.str.36 = private unnamed_addr constant [17 x i8] c"java/lang/String\00", align 16
@.str.37 = private unnamed_addr constant [17 x i8] c"java/lang/Thread\00", align 16
@.str.38 = private unnamed_addr constant [35 x i8] c"mono/java/lang/RunnableImplementor\00", align 16
@.str.39 = private unnamed_addr constant [20 x i8] c"java/lang/Throwable\00", align 16
@.str.40 = private unnamed_addr constant [25 x i8] c"mono/android/TypeManager\00", align 16
@.str.41 = private unnamed_addr constant [35 x i8] c"crc64b79b9ba72943b64e/MainActivity\00", align 16

;TypeMapModule
@.TypeMapModule.0_assembly_name = private unnamed_addr constant [13 x i8] c"Mono.Android\00", align 1
@.TypeMapModule.1_assembly_name = private unnamed_addr constant [18 x i8] c"GridForge.Android\00", align 16

; Metadata
!llvm.module.flags = !{!0, !1}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
