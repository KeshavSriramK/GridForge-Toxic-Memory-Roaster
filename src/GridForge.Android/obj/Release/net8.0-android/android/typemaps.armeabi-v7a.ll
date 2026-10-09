; ModuleID = 'typemaps.armeabi-v7a.ll'
source_filename = "typemaps.armeabi-v7a.ll"
target datalayout = "e-m:e-p:32:32-Fi8-i64:64-v128:64:128-a:0:32-n32-S64"
target triple = "armv7-unknown-linux-android21"

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
		[16 x i8] c"H\A1\C3\A1n\E8fJ\BA\B1\AC\F9\05k\0D~", ; module_uuid: a1c3a148-e86e-4a66-bab1-acf9056b0d7e
		i32 1, ; uint32_t entry_count (0x1)
		i32 0, ; uint32_t duplicate_count (0x0)
		ptr @module0_managed_to_java, ; TypeMapModuleEntry* map
		ptr null, ; TypeMapModuleEntry* duplicate_map
		ptr @.TypeMapModule.0_assembly_name, ; assembly_name: GridForge.Android
		ptr null, ; MonoImage* image
		i32 0, ; uint32_t java_name_width (0x0)
		ptr null; uint8_t* java_map (0x0)
	}, ; 0
	%struct.TypeMapModule {
		[16 x i8] c"\8A\04T\D1\E3\B9\E4N\9D4\9Bg7\91~\96", ; module_uuid: d154048a-b9e3-4ee4-9d34-9b6737917e96
		i32 41, ; uint32_t entry_count (0x29)
		i32 10, ; uint32_t duplicate_count (0xa)
		ptr @module1_managed_to_java, ; TypeMapModuleEntry* map
		ptr @module1_managed_to_java_duplicates, ; TypeMapModuleEntry* duplicate_map
		ptr @.TypeMapModule.1_assembly_name, ; assembly_name: Mono.Android
		ptr null, ; MonoImage* image
		i32 0, ; uint32_t java_name_width (0x0)
		ptr null; uint8_t* java_map (0x0)
	} ; 1
], align 4

; Java types name hashes
@map_java_hashes = dso_local local_unnamed_addr constant [42 x i32] [
	i32 12341354, ; 0: 0xbc506a => java/lang/Object
	i32 74282880, ; 1: 0x46d7780 => android/view/ViewGroup
	i32 393371378, ; 2: 0x17725ef2 => mono/java/lang/RunnableImplementor
	i32 531198748, ; 3: 0x1fa9731c => mono/android/runtime/OutputStreamAdapter
	i32 581097368, ; 4: 0x22a2d798 => java/nio/channels/FileChannel
	i32 591810476, ; 5: 0x23464fac => android/os/Bundle
	i32 780987551, ; 6: 0x2e8cec9f => java/io/PrintWriter
	i32 806800039, ; 7: 0x3016caa7 => java/lang/Thread
	i32 1008962460, ; 8: 0x3c238b9c => android/graphics/Color
	i32 1298454265, ; 9: 0x4d64d6f9 => java/lang/Throwable
	i32 1489594546, ; 10: 0x58c968b2 => java/nio/channels/spi/AbstractInterruptibleChannel
	i32 1586851388, ; 11: 0x5e956e3c => android/os/Handler
	i32 1646348278, ; 12: 0x622147f6 => android/view/View
	i32 1758490869, ; 13: 0x68d070f5 => android/os/BaseBundle
	i32 1851730788, ; 14: 0x6e5f2b64 => java/lang/Runnable
	i32 1859010077, ; 15: 0x6ece3e1d => android/widget/LinearLayout
	i32 1944129628, ; 16: 0x73e1105c => java/io/OutputStream
	i32 1985929388, ; 17: 0x765ee0ac => android/app/Activity
	i32 2027782872, ; 18: 0x78dd82d8 => android/view/ContextThemeWrapper
	i32 2284656609, ; 19: 0x882d17e1 => android/app/Application
	i32 2363729366, ; 20: 0x8ce3a5d6 => java/lang/Enum
	i32 2558143838, ; 21: 0x987a2d5e => java/io/FileInputStream
	i32 2578357124, ; 22: 0x99ae9b84 => android/widget/ImageView$ScaleType
	i32 2687778660, ; 23: 0xa0343f64 => android/widget/TextView
	i32 2874673969, ; 24: 0xab580b31 => java/lang/StackTraceElement
	i32 2933762856, ; 25: 0xaeddab28 => android/util/AttributeSet
	i32 2942792700, ; 26: 0xaf6773fc => java/lang/Exception
	i32 2983720033, ; 27: 0xb1d7f461 => mono/android/TypeManager
	i32 3032808825, ; 28: 0xb4c4fd79 => java/io/StringWriter
	i32 3576242387, ; 29: 0xd52920d3 => android/runtime/JavaProxyThrowable
	i32 3666243682, ; 30: 0xda867062 => java/lang/String
	i32 3882570516, ; 31: 0xe76b5314 => java/lang/Class
	i32 3896288302, ; 32: 0xe83ca42e => android/widget/ImageView
	i32 3900581163, ; 33: 0xe87e252b => java/io/InputStream
	i32 3943616452, ; 34: 0xeb0ecfc4 => crc64b79b9ba72943b64e/MainActivity
	i32 3969984744, ; 35: 0xeca128e8 => mono/android/runtime/InputStreamAdapter
	i32 3993327007, ; 36: 0xee05559f => android/content/ContextWrapper
	i32 4020308495, ; 37: 0xefa10a0f => java/lang/Error
	i32 4051772911, ; 38: 0xf18125ef => android/content/Context
	i32 4101363546, ; 39: 0xf475d75a => java/io/Writer
	i32 4118878202, ; 40: 0xf58117fa => android/os/Looper
	i32 4157808693 ; 41: 0xf7d32035 => java/io/IOException
], align 4

@module0_managed_to_java = internal dso_local constant [1 x %struct.TypeMapModuleEntry] [
	%struct.TypeMapModuleEntry {
		i32 33554434, ; uint32_t type_token_id (0x2000002)
		i32 34; uint32_t java_map_index (0x22)
	} ; 0
], align 4

@module1_managed_to_java = internal dso_local constant [41 x %struct.TypeMapModuleEntry] [
	%struct.TypeMapModuleEntry {
		i32 33554488, ; uint32_t type_token_id (0x2000038)
		i32 32; uint32_t java_map_index (0x20)
	}, ; 0
	%struct.TypeMapModuleEntry {
		i32 33554489, ; uint32_t type_token_id (0x2000039)
		i32 22; uint32_t java_map_index (0x16)
	}, ; 1
	%struct.TypeMapModuleEntry {
		i32 33554490, ; uint32_t type_token_id (0x200003a)
		i32 15; uint32_t java_map_index (0xf)
	}, ; 2
	%struct.TypeMapModuleEntry {
		i32 33554491, ; uint32_t type_token_id (0x200003b)
		i32 23; uint32_t java_map_index (0x17)
	}, ; 3
	%struct.TypeMapModuleEntry {
		i32 33554493, ; uint32_t type_token_id (0x200003d)
		i32 25; uint32_t java_map_index (0x19)
	}, ; 4
	%struct.TypeMapModuleEntry {
		i32 33554495, ; uint32_t type_token_id (0x200003f)
		i32 13; uint32_t java_map_index (0xd)
	}, ; 5
	%struct.TypeMapModuleEntry {
		i32 33554496, ; uint32_t type_token_id (0x2000040)
		i32 5; uint32_t java_map_index (0x5)
	}, ; 6
	%struct.TypeMapModuleEntry {
		i32 33554497, ; uint32_t type_token_id (0x2000041)
		i32 11; uint32_t java_map_index (0xb)
	}, ; 7
	%struct.TypeMapModuleEntry {
		i32 33554498, ; uint32_t type_token_id (0x2000042)
		i32 40; uint32_t java_map_index (0x28)
	}, ; 8
	%struct.TypeMapModuleEntry {
		i32 33554500, ; uint32_t type_token_id (0x2000044)
		i32 17; uint32_t java_map_index (0x11)
	}, ; 9
	%struct.TypeMapModuleEntry {
		i32 33554501, ; uint32_t type_token_id (0x2000045)
		i32 19; uint32_t java_map_index (0x13)
	}, ; 10
	%struct.TypeMapModuleEntry {
		i32 33554504, ; uint32_t type_token_id (0x2000048)
		i32 18; uint32_t java_map_index (0x12)
	}, ; 11
	%struct.TypeMapModuleEntry {
		i32 33554505, ; uint32_t type_token_id (0x2000049)
		i32 12; uint32_t java_map_index (0xc)
	}, ; 12
	%struct.TypeMapModuleEntry {
		i32 33554506, ; uint32_t type_token_id (0x200004a)
		i32 1; uint32_t java_map_index (0x1)
	}, ; 13
	%struct.TypeMapModuleEntry {
		i32 33554527, ; uint32_t type_token_id (0x200005f)
		i32 35; uint32_t java_map_index (0x23)
	}, ; 14
	%struct.TypeMapModuleEntry {
		i32 33554529, ; uint32_t type_token_id (0x2000061)
		i32 29; uint32_t java_map_index (0x1d)
	}, ; 15
	%struct.TypeMapModuleEntry {
		i32 33554540, ; uint32_t type_token_id (0x200006c)
		i32 3; uint32_t java_map_index (0x3)
	}, ; 16
	%struct.TypeMapModuleEntry {
		i32 33554546, ; uint32_t type_token_id (0x2000072)
		i32 8; uint32_t java_map_index (0x8)
	}, ; 17
	%struct.TypeMapModuleEntry {
		i32 33554549, ; uint32_t type_token_id (0x2000075)
		i32 38; uint32_t java_map_index (0x26)
	}, ; 18
	%struct.TypeMapModuleEntry {
		i32 33554551, ; uint32_t type_token_id (0x2000077)
		i32 36; uint32_t java_map_index (0x24)
	}, ; 19
	%struct.TypeMapModuleEntry {
		i32 33554553, ; uint32_t type_token_id (0x2000079)
		i32 4; uint32_t java_map_index (0x4)
	}, ; 20
	%struct.TypeMapModuleEntry {
		i32 33554555, ; uint32_t type_token_id (0x200007b)
		i32 10; uint32_t java_map_index (0xa)
	}, ; 21
	%struct.TypeMapModuleEntry {
		i32 33554557, ; uint32_t type_token_id (0x200007d)
		i32 21; uint32_t java_map_index (0x15)
	}, ; 22
	%struct.TypeMapModuleEntry {
		i32 33554558, ; uint32_t type_token_id (0x200007e)
		i32 33; uint32_t java_map_index (0x21)
	}, ; 23
	%struct.TypeMapModuleEntry {
		i32 33554560, ; uint32_t type_token_id (0x2000080)
		i32 41; uint32_t java_map_index (0x29)
	}, ; 24
	%struct.TypeMapModuleEntry {
		i32 33554561, ; uint32_t type_token_id (0x2000081)
		i32 16; uint32_t java_map_index (0x10)
	}, ; 25
	%struct.TypeMapModuleEntry {
		i32 33554563, ; uint32_t type_token_id (0x2000083)
		i32 6; uint32_t java_map_index (0x6)
	}, ; 26
	%struct.TypeMapModuleEntry {
		i32 33554564, ; uint32_t type_token_id (0x2000084)
		i32 28; uint32_t java_map_index (0x1c)
	}, ; 27
	%struct.TypeMapModuleEntry {
		i32 33554565, ; uint32_t type_token_id (0x2000085)
		i32 39; uint32_t java_map_index (0x27)
	}, ; 28
	%struct.TypeMapModuleEntry {
		i32 33554567, ; uint32_t type_token_id (0x2000087)
		i32 31; uint32_t java_map_index (0x1f)
	}, ; 29
	%struct.TypeMapModuleEntry {
		i32 33554568, ; uint32_t type_token_id (0x2000088)
		i32 20; uint32_t java_map_index (0x14)
	}, ; 30
	%struct.TypeMapModuleEntry {
		i32 33554570, ; uint32_t type_token_id (0x200008a)
		i32 37; uint32_t java_map_index (0x25)
	}, ; 31
	%struct.TypeMapModuleEntry {
		i32 33554571, ; uint32_t type_token_id (0x200008b)
		i32 26; uint32_t java_map_index (0x1a)
	}, ; 32
	%struct.TypeMapModuleEntry {
		i32 33554572, ; uint32_t type_token_id (0x200008c)
		i32 14; uint32_t java_map_index (0xe)
	}, ; 33
	%struct.TypeMapModuleEntry {
		i32 33554574, ; uint32_t type_token_id (0x200008e)
		i32 0; uint32_t java_map_index (0x0)
	}, ; 34
	%struct.TypeMapModuleEntry {
		i32 33554575, ; uint32_t type_token_id (0x200008f)
		i32 24; uint32_t java_map_index (0x18)
	}, ; 35
	%struct.TypeMapModuleEntry {
		i32 33554576, ; uint32_t type_token_id (0x2000090)
		i32 30; uint32_t java_map_index (0x1e)
	}, ; 36
	%struct.TypeMapModuleEntry {
		i32 33554578, ; uint32_t type_token_id (0x2000092)
		i32 7; uint32_t java_map_index (0x7)
	}, ; 37
	%struct.TypeMapModuleEntry {
		i32 33554579, ; uint32_t type_token_id (0x2000093)
		i32 2; uint32_t java_map_index (0x2)
	}, ; 38
	%struct.TypeMapModuleEntry {
		i32 33554580, ; uint32_t type_token_id (0x2000094)
		i32 9; uint32_t java_map_index (0x9)
	}, ; 39
	%struct.TypeMapModuleEntry {
		i32 33554591, ; uint32_t type_token_id (0x200009f)
		i32 27; uint32_t java_map_index (0x1b)
	} ; 40
], align 4

@module1_managed_to_java_duplicates = internal dso_local constant [10 x %struct.TypeMapModuleEntry] [
	%struct.TypeMapModuleEntry {
		i32 33554494, ; uint32_t type_token_id (0x200003e)
		i32 25; uint32_t java_map_index (0x19)
	}, ; 0
	%struct.TypeMapModuleEntry {
		i32 33554507, ; uint32_t type_token_id (0x200004b)
		i32 1; uint32_t java_map_index (0x1)
	}, ; 1
	%struct.TypeMapModuleEntry {
		i32 33554550, ; uint32_t type_token_id (0x2000076)
		i32 38; uint32_t java_map_index (0x26)
	}, ; 2
	%struct.TypeMapModuleEntry {
		i32 33554554, ; uint32_t type_token_id (0x200007a)
		i32 4; uint32_t java_map_index (0x4)
	}, ; 3
	%struct.TypeMapModuleEntry {
		i32 33554556, ; uint32_t type_token_id (0x200007c)
		i32 10; uint32_t java_map_index (0xa)
	}, ; 4
	%struct.TypeMapModuleEntry {
		i32 33554559, ; uint32_t type_token_id (0x200007f)
		i32 33; uint32_t java_map_index (0x21)
	}, ; 5
	%struct.TypeMapModuleEntry {
		i32 33554562, ; uint32_t type_token_id (0x2000082)
		i32 16; uint32_t java_map_index (0x10)
	}, ; 6
	%struct.TypeMapModuleEntry {
		i32 33554566, ; uint32_t type_token_id (0x2000086)
		i32 39; uint32_t java_map_index (0x27)
	}, ; 7
	%struct.TypeMapModuleEntry {
		i32 33554569, ; uint32_t type_token_id (0x2000089)
		i32 20; uint32_t java_map_index (0x14)
	}, ; 8
	%struct.TypeMapModuleEntry {
		i32 33554573, ; uint32_t type_token_id (0x200008d)
		i32 14; uint32_t java_map_index (0xe)
	} ; 9
], align 4

; Java to managed map
@map_java = dso_local local_unnamed_addr constant [42 x %struct.TypeMapJava] [
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554574, ; uint32_t type_token_id (0x200008e)
		i32 35; uint32_t java_name_index (0x23)
	}, ; 0
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554506, ; uint32_t type_token_id (0x200004a)
		i32 14; uint32_t java_name_index (0xe)
	}, ; 1
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554579, ; uint32_t type_token_id (0x2000093)
		i32 39; uint32_t java_name_index (0x27)
	}, ; 2
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554540, ; uint32_t type_token_id (0x200006c)
		i32 17; uint32_t java_name_index (0x11)
	}, ; 3
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554553, ; uint32_t type_token_id (0x2000079)
		i32 21; uint32_t java_name_index (0x15)
	}, ; 4
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554496, ; uint32_t type_token_id (0x2000040)
		i32 7; uint32_t java_name_index (0x7)
	}, ; 5
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554563, ; uint32_t type_token_id (0x2000083)
		i32 27; uint32_t java_name_index (0x1b)
	}, ; 6
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554578, ; uint32_t type_token_id (0x2000092)
		i32 38; uint32_t java_name_index (0x26)
	}, ; 7
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554546, ; uint32_t type_token_id (0x2000072)
		i32 18; uint32_t java_name_index (0x12)
	}, ; 8
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554580, ; uint32_t type_token_id (0x2000094)
		i32 40; uint32_t java_name_index (0x28)
	}, ; 9
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554555, ; uint32_t type_token_id (0x200007b)
		i32 22; uint32_t java_name_index (0x16)
	}, ; 10
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554497, ; uint32_t type_token_id (0x2000041)
		i32 8; uint32_t java_name_index (0x8)
	}, ; 11
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554505, ; uint32_t type_token_id (0x2000049)
		i32 13; uint32_t java_name_index (0xd)
	}, ; 12
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554495, ; uint32_t type_token_id (0x200003f)
		i32 6; uint32_t java_name_index (0x6)
	}, ; 13
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 0, ; uint32_t type_token_id (0x0)
		i32 34; uint32_t java_name_index (0x22)
	}, ; 14
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554490, ; uint32_t type_token_id (0x200003a)
		i32 3; uint32_t java_name_index (0x3)
	}, ; 15
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554561, ; uint32_t type_token_id (0x2000081)
		i32 26; uint32_t java_name_index (0x1a)
	}, ; 16
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554500, ; uint32_t type_token_id (0x2000044)
		i32 10; uint32_t java_name_index (0xa)
	}, ; 17
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554504, ; uint32_t type_token_id (0x2000048)
		i32 12; uint32_t java_name_index (0xc)
	}, ; 18
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554501, ; uint32_t type_token_id (0x2000045)
		i32 11; uint32_t java_name_index (0xb)
	}, ; 19
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554568, ; uint32_t type_token_id (0x2000088)
		i32 31; uint32_t java_name_index (0x1f)
	}, ; 20
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554557, ; uint32_t type_token_id (0x200007d)
		i32 23; uint32_t java_name_index (0x17)
	}, ; 21
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554489, ; uint32_t type_token_id (0x2000039)
		i32 2; uint32_t java_name_index (0x2)
	}, ; 22
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554491, ; uint32_t type_token_id (0x200003b)
		i32 4; uint32_t java_name_index (0x4)
	}, ; 23
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554575, ; uint32_t type_token_id (0x200008f)
		i32 36; uint32_t java_name_index (0x24)
	}, ; 24
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 0, ; uint32_t type_token_id (0x0)
		i32 5; uint32_t java_name_index (0x5)
	}, ; 25
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554571, ; uint32_t type_token_id (0x200008b)
		i32 33; uint32_t java_name_index (0x21)
	}, ; 26
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554591, ; uint32_t type_token_id (0x200009f)
		i32 41; uint32_t java_name_index (0x29)
	}, ; 27
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554564, ; uint32_t type_token_id (0x2000084)
		i32 28; uint32_t java_name_index (0x1c)
	}, ; 28
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554529, ; uint32_t type_token_id (0x2000061)
		i32 16; uint32_t java_name_index (0x10)
	}, ; 29
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554576, ; uint32_t type_token_id (0x2000090)
		i32 37; uint32_t java_name_index (0x25)
	}, ; 30
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554567, ; uint32_t type_token_id (0x2000087)
		i32 30; uint32_t java_name_index (0x1e)
	}, ; 31
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554488, ; uint32_t type_token_id (0x2000038)
		i32 1; uint32_t java_name_index (0x1)
	}, ; 32
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554558, ; uint32_t type_token_id (0x200007e)
		i32 24; uint32_t java_name_index (0x18)
	}, ; 33
	%struct.TypeMapJava {
		i32 0, ; uint32_t module_index (0x0)
		i32 33554434, ; uint32_t type_token_id (0x2000002)
		i32 0; uint32_t java_name_index (0x0)
	}, ; 34
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554527, ; uint32_t type_token_id (0x200005f)
		i32 15; uint32_t java_name_index (0xf)
	}, ; 35
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554551, ; uint32_t type_token_id (0x2000077)
		i32 20; uint32_t java_name_index (0x14)
	}, ; 36
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554570, ; uint32_t type_token_id (0x200008a)
		i32 32; uint32_t java_name_index (0x20)
	}, ; 37
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554549, ; uint32_t type_token_id (0x2000075)
		i32 19; uint32_t java_name_index (0x13)
	}, ; 38
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554565, ; uint32_t type_token_id (0x2000085)
		i32 29; uint32_t java_name_index (0x1d)
	}, ; 39
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554498, ; uint32_t type_token_id (0x2000042)
		i32 9; uint32_t java_name_index (0x9)
	}, ; 40
	%struct.TypeMapJava {
		i32 1, ; uint32_t module_index (0x1)
		i32 33554560, ; uint32_t type_token_id (0x2000080)
		i32 25; uint32_t java_name_index (0x19)
	} ; 41
], align 4

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
], align 4

; Strings
@.str.0 = private unnamed_addr constant [35 x i8] c"crc64b79b9ba72943b64e/MainActivity\00", align 1
@.str.1 = private unnamed_addr constant [25 x i8] c"android/widget/ImageView\00", align 1
@.str.2 = private unnamed_addr constant [35 x i8] c"android/widget/ImageView$ScaleType\00", align 1
@.str.3 = private unnamed_addr constant [28 x i8] c"android/widget/LinearLayout\00", align 1
@.str.4 = private unnamed_addr constant [24 x i8] c"android/widget/TextView\00", align 1
@.str.5 = private unnamed_addr constant [26 x i8] c"android/util/AttributeSet\00", align 1
@.str.6 = private unnamed_addr constant [22 x i8] c"android/os/BaseBundle\00", align 1
@.str.7 = private unnamed_addr constant [18 x i8] c"android/os/Bundle\00", align 1
@.str.8 = private unnamed_addr constant [19 x i8] c"android/os/Handler\00", align 1
@.str.9 = private unnamed_addr constant [18 x i8] c"android/os/Looper\00", align 1
@.str.10 = private unnamed_addr constant [21 x i8] c"android/app/Activity\00", align 1
@.str.11 = private unnamed_addr constant [24 x i8] c"android/app/Application\00", align 1
@.str.12 = private unnamed_addr constant [33 x i8] c"android/view/ContextThemeWrapper\00", align 1
@.str.13 = private unnamed_addr constant [18 x i8] c"android/view/View\00", align 1
@.str.14 = private unnamed_addr constant [23 x i8] c"android/view/ViewGroup\00", align 1
@.str.15 = private unnamed_addr constant [40 x i8] c"mono/android/runtime/InputStreamAdapter\00", align 1
@.str.16 = private unnamed_addr constant [35 x i8] c"android/runtime/JavaProxyThrowable\00", align 1
@.str.17 = private unnamed_addr constant [41 x i8] c"mono/android/runtime/OutputStreamAdapter\00", align 1
@.str.18 = private unnamed_addr constant [23 x i8] c"android/graphics/Color\00", align 1
@.str.19 = private unnamed_addr constant [24 x i8] c"android/content/Context\00", align 1
@.str.20 = private unnamed_addr constant [31 x i8] c"android/content/ContextWrapper\00", align 1
@.str.21 = private unnamed_addr constant [30 x i8] c"java/nio/channels/FileChannel\00", align 1
@.str.22 = private unnamed_addr constant [51 x i8] c"java/nio/channels/spi/AbstractInterruptibleChannel\00", align 1
@.str.23 = private unnamed_addr constant [24 x i8] c"java/io/FileInputStream\00", align 1
@.str.24 = private unnamed_addr constant [20 x i8] c"java/io/InputStream\00", align 1
@.str.25 = private unnamed_addr constant [20 x i8] c"java/io/IOException\00", align 1
@.str.26 = private unnamed_addr constant [21 x i8] c"java/io/OutputStream\00", align 1
@.str.27 = private unnamed_addr constant [20 x i8] c"java/io/PrintWriter\00", align 1
@.str.28 = private unnamed_addr constant [21 x i8] c"java/io/StringWriter\00", align 1
@.str.29 = private unnamed_addr constant [15 x i8] c"java/io/Writer\00", align 1
@.str.30 = private unnamed_addr constant [16 x i8] c"java/lang/Class\00", align 1
@.str.31 = private unnamed_addr constant [15 x i8] c"java/lang/Enum\00", align 1
@.str.32 = private unnamed_addr constant [16 x i8] c"java/lang/Error\00", align 1
@.str.33 = private unnamed_addr constant [20 x i8] c"java/lang/Exception\00", align 1
@.str.34 = private unnamed_addr constant [19 x i8] c"java/lang/Runnable\00", align 1
@.str.35 = private unnamed_addr constant [17 x i8] c"java/lang/Object\00", align 1
@.str.36 = private unnamed_addr constant [28 x i8] c"java/lang/StackTraceElement\00", align 1
@.str.37 = private unnamed_addr constant [17 x i8] c"java/lang/String\00", align 1
@.str.38 = private unnamed_addr constant [17 x i8] c"java/lang/Thread\00", align 1
@.str.39 = private unnamed_addr constant [35 x i8] c"mono/java/lang/RunnableImplementor\00", align 1
@.str.40 = private unnamed_addr constant [20 x i8] c"java/lang/Throwable\00", align 1
@.str.41 = private unnamed_addr constant [25 x i8] c"mono/android/TypeManager\00", align 1

;TypeMapModule
@.TypeMapModule.0_assembly_name = private unnamed_addr constant [18 x i8] c"GridForge.Android\00", align 1
@.TypeMapModule.1_assembly_name = private unnamed_addr constant [13 x i8] c"Mono.Android\00", align 1

; Metadata
!llvm.module.flags = !{!0, !1, !7}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"min_enum_size", i32 4}
