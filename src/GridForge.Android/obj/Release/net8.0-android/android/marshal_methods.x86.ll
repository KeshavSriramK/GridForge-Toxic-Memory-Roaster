; ModuleID = 'marshal_methods.x86.ll'
source_filename = "marshal_methods.x86.ll"
target datalayout = "e-m:e-p:32:32-p270:32:32-p271:32:32-p272:64:64-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i686-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [15 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [30 x i32] [
	i32 117431740, ; 0: System.Runtime.InteropServices => 0x6ffddbc => 9
	i32 395744057, ; 1: _Microsoft.Android.Resource.Designer => 0x17969339 => 0
	i32 439010522, ; 2: GridForge.Core => 0x1a2ac4da => 2
	i32 442565967, ; 3: System.Collections => 0x1a61054f => 4
	i32 992768348, ; 4: System.Collections.dll => 0x3b2c715c => 4
	i32 1106948985, ; 5: Raylib-cs => 0x41fab379 => 1
	i32 1324164729, ; 6: System.Linq => 0x4eed2679 => 6
	i32 1657153582, ; 7: System.Runtime => 0x62c6282e => 10
	i32 1780572499, ; 8: Mono.Android.Runtime.dll => 0x6a216153 => 13
	i32 2079903147, ; 9: System.Runtime.dll => 0x7bf8cdab => 10
	i32 2090596640, ; 10: System.Numerics.Vectors => 0x7c9bf920 => 7
	i32 2127167465, ; 11: System.Console => 0x7ec9ffe9 => 5
	i32 2305521784, ; 12: System.Private.CoreLib.dll => 0x896b7878 => 11
	i32 2340441535, ; 13: System.Runtime.InteropServices.RuntimeInformation.dll => 0x8b804dbf => 8
	i32 2435356389, ; 14: System.Console.dll => 0x912896e5 => 5
	i32 2475788418, ; 15: Java.Interop.dll => 0x93918882 => 12
	i32 2649478774, ; 16: GridForge.Core.dll => 0x9debd676 => 2
	i32 2649790485, ; 17: GridForge.Android.dll => 0x9df09815 => 3
	i32 2909740682, ; 18: System.Private.CoreLib => 0xad6f1e8a => 11
	i32 2919462931, ; 19: System.Numerics.Vectors.dll => 0xae037813 => 7
	i32 3038032645, ; 20: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 0
	i32 3059408633, ; 21: Mono.Android.Runtime => 0xb65adef9 => 13
	i32 3366347497, ; 22: Java.Interop => 0xc8a662e9 => 12
	i32 3476120550, ; 23: Mono.Android => 0xcf3163e6 => 14
	i32 3608519521, ; 24: System.Linq.dll => 0xd715a361 => 6
	i32 3624195450, ; 25: System.Runtime.InteropServices.RuntimeInformation => 0xd804d57a => 8
	i32 3672681054, ; 26: Mono.Android.dll => 0xdae8aa5e => 14
	i32 3776118834, ; 27: Raylib-cs.dll => 0xe1130032 => 1
	i32 3849253459, ; 28: System.Runtime.InteropServices.dll => 0xe56ef253 => 9
	i32 4072035376 ; 29: GridForge.Android => 0xf2b65430 => 3
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [30 x i32] [
	i32 9, ; 0
	i32 0, ; 1
	i32 2, ; 2
	i32 4, ; 3
	i32 4, ; 4
	i32 1, ; 5
	i32 6, ; 6
	i32 10, ; 7
	i32 13, ; 8
	i32 10, ; 9
	i32 7, ; 10
	i32 5, ; 11
	i32 11, ; 12
	i32 8, ; 13
	i32 5, ; 14
	i32 12, ; 15
	i32 2, ; 16
	i32 3, ; 17
	i32 11, ; 18
	i32 7, ; 19
	i32 0, ; 20
	i32 13, ; 21
	i32 12, ; 22
	i32 14, ; 23
	i32 6, ; 24
	i32 8, ; 25
	i32 14, ; 26
	i32 1, ; 27
	i32 9, ; 28
	i32 3 ; 29
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 4

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 4

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 4

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 4, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" }

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
!7 = !{i32 1, !"NumRegisterParameters", i32 0}
