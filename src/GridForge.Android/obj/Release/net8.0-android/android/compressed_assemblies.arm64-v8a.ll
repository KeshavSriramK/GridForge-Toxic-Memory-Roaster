; ModuleID = 'compressed_assemblies.arm64-v8a.ll'
source_filename = "compressed_assemblies.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.CompressedAssemblies = type {
	i32, ; uint32_t count
	ptr ; CompressedAssemblyDescriptor descriptors
}

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i8, ; bool loaded
	ptr ; uint8_t data
}

@compressed_assemblies = dso_local local_unnamed_addr global %struct.CompressedAssemblies {
	i32 18, ; uint32_t count (0x12)
	ptr @compressed_assembly_descriptors; CompressedAssemblyDescriptor* descriptors
}, align 8

@compressed_assembly_descriptors = internal dso_local global [18 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size (0x1600)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_0; uint8_t* data (0x0)
	}, ; 0
	%struct.CompressedAssemblyDescriptor {
		i32 27648, ; uint32_t uncompressed_file_size (0x6c00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_1; uint8_t* data (0x0)
	}, ; 1
	%struct.CompressedAssemblyDescriptor {
		i32 129024, ; uint32_t uncompressed_file_size (0x1f800)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_2; uint8_t* data (0x0)
	}, ; 2
	%struct.CompressedAssemblyDescriptor {
		i32 18984, ; uint32_t uncompressed_file_size (0x4a28)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_3; uint8_t* data (0x0)
	}, ; 3
	%struct.CompressedAssemblyDescriptor {
		i32 193536, ; uint32_t uncompressed_file_size (0x2f400)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_4; uint8_t* data (0x0)
	}, ; 4
	%struct.CompressedAssemblyDescriptor {
		i32 89600, ; uint32_t uncompressed_file_size (0x15e00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_5; uint8_t* data (0x0)
	}, ; 5
	%struct.CompressedAssemblyDescriptor {
		i32 10240, ; uint32_t uncompressed_file_size (0x2800)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_6; uint8_t* data (0x0)
	}, ; 6
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size (0x2a00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_7; uint8_t* data (0x0)
	}, ; 7
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size (0x3e00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_8; uint8_t* data (0x0)
	}, ; 8
	%struct.CompressedAssemblyDescriptor {
		i32 4608, ; uint32_t uncompressed_file_size (0x1200)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_9; uint8_t* data (0x0)
	}, ; 9
	%struct.CompressedAssemblyDescriptor {
		i32 4096, ; uint32_t uncompressed_file_size (0x1000)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_10; uint8_t* data (0x0)
	}, ; 10
	%struct.CompressedAssemblyDescriptor {
		i32 8192, ; uint32_t uncompressed_file_size (0x2000)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_11; uint8_t* data (0x0)
	}, ; 11
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size (0x1a00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_12; uint8_t* data (0x0)
	}, ; 12
	%struct.CompressedAssemblyDescriptor {
		i32 2560, ; uint32_t uncompressed_file_size (0xa00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_13; uint8_t* data (0x0)
	}, ; 13
	%struct.CompressedAssemblyDescriptor {
		i32 1117184, ; uint32_t uncompressed_file_size (0x110c00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_14; uint8_t* data (0x0)
	}, ; 14
	%struct.CompressedAssemblyDescriptor {
		i32 1103872, ; uint32_t uncompressed_file_size (0x10d800)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_15; uint8_t* data (0x0)
	}, ; 15
	%struct.CompressedAssemblyDescriptor {
		i32 1103872, ; uint32_t uncompressed_file_size (0x10d800)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_16; uint8_t* data (0x0)
	}, ; 16
	%struct.CompressedAssemblyDescriptor {
		i32 1198592, ; uint32_t uncompressed_file_size (0x124a00)
		i8 0, ; bool loaded
		ptr @__compressedAssemblyData_17; uint8_t* data (0x0)
	} ; 17
], align 8

@__compressedAssemblyData_0 = internal dso_local global [5632 x i8] zeroinitializer, align 1
@__compressedAssemblyData_1 = internal dso_local global [27648 x i8] zeroinitializer, align 1
@__compressedAssemblyData_2 = internal dso_local global [129024 x i8] zeroinitializer, align 1
@__compressedAssemblyData_3 = internal dso_local global [18984 x i8] zeroinitializer, align 1
@__compressedAssemblyData_4 = internal dso_local global [193536 x i8] zeroinitializer, align 1
@__compressedAssemblyData_5 = internal dso_local global [89600 x i8] zeroinitializer, align 1
@__compressedAssemblyData_6 = internal dso_local global [10240 x i8] zeroinitializer, align 1
@__compressedAssemblyData_7 = internal dso_local global [10752 x i8] zeroinitializer, align 1
@__compressedAssemblyData_8 = internal dso_local global [15872 x i8] zeroinitializer, align 1
@__compressedAssemblyData_9 = internal dso_local global [4608 x i8] zeroinitializer, align 1
@__compressedAssemblyData_10 = internal dso_local global [4096 x i8] zeroinitializer, align 1
@__compressedAssemblyData_11 = internal dso_local global [8192 x i8] zeroinitializer, align 1
@__compressedAssemblyData_12 = internal dso_local global [6656 x i8] zeroinitializer, align 1
@__compressedAssemblyData_13 = internal dso_local global [2560 x i8] zeroinitializer, align 1
@__compressedAssemblyData_14 = internal dso_local global [1117184 x i8] zeroinitializer, align 1
@__compressedAssemblyData_15 = internal dso_local global [1103872 x i8] zeroinitializer, align 1
@__compressedAssemblyData_16 = internal dso_local global [1103872 x i8] zeroinitializer, align 1
@__compressedAssemblyData_17 = internal dso_local global [1198592 x i8] zeroinitializer, align 1

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
