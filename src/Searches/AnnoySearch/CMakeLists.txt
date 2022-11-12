cmake_minimum_required(VERSION 3.0.0)
cmake_policy(SET CMP0053 NEW)
cmake_policy(SET CMP0054 NEW)

set(PROJ_NAME FaceRecognitionDotNetNativeAnnoySearch)

project(${PROJ_NAME} VERSION 1.0.0)

# OS info
message("-------------------------------------------------------")
message("-- CMAKE_SYSTEM_INFO_FILE: ${CMAKE_SYSTEM_INFO_FILE}")
message("-- CMAKE_SYSTEM_NAME:      ${CMAKE_SYSTEM_NAME}")
message("-- CMAKE_SYSTEM_PROCESSOR: ${CMAKE_SYSTEM_PROCESSOR}")
message("-- CMAKE_SYSTEM:           ${CMAKE_SYSTEM}")
message("-- CMAKE_C_COMPILER:       ${CMAKE_C_COMPILER}")
message("-- CMAKE_CXX_COMPILER:     ${CMAKE_CXX_COMPILER}")
string (REGEX MATCH "\\.el[1-9]" os_version_suffix ${CMAKE_SYSTEM})
message("-- os_version_suffix:      ${os_version_suffix}")
message("-- Project: ${CMAKE_CURRENT_SOURCE_DIR}")
message("-- Binaries: ${CMAKE_CURRENT_BINARY_DIR}")
message("-------------------------------------------------------")

# Version info
set(VERSION_MAJOR 1)
set(VERSION_MINOR 3)
set(VERSION_PATCH 0)
set(VERSION_DATE  7)

# Only GCC requires -fPIC
if ("${CMAKE_CXX_COMPILER_ID}" STREQUAL "GNU")
	set(CMAKE_C_FLAGS "-fPIC")
	set(CMAKE_CXX_FLAGS "-fPIC")
endif()

# set architecture
if (NOT ${CMAKE_GENERATOR} MATCHES "Visual Studio")
    if ("${ARCH_TYPE}" STREQUAL "32")
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -m32")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -m32")
    elseif ("${ARCH_TYPE}" STREQUAL "64")
        set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -m64")
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -m64")
    endif()
endif()

# Select the release build type by default
if (NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE "Release" CACHE STRING "" FORCE)
    message("-- CMAKE_BUILD_TYPE: ${CMAKE_BUILD_TYPE}")
endif()

# Set /bigobj for MSVC
if (${CMAKE_GENERATOR} MATCHES "Visual Studio")
	# C4819: The file contains a character that cannot be represented in the current code page
	# C4503: identifier' : decorated name length exceeded, name was truncated
	set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /wd4819 /wd4503 /bigobj")

	# C4146: unary minus operator applied to unsigned type, result still unsigned
	if ("${CMAKE_SYSTEM_NAME}" STREQUAL "WindowsStore")
		set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /wd4146 /ZW /EHsc")
	endif()
endif()

# create config file
configure_file(
    ${PROJECT_SOURCE_DIR}/src/config.h.in
    ${PROJECT_SOURCE_DIR}/src/config.h
    @ONLY)

FILE(GLOB_RECURSE HEADERS ${PROJECT_SOURCE_DIR}/src/*.h)
FILE(GLOB_RECURSE SOURCES ${PROJECT_SOURCE_DIR}/src/*.cpp)

set(INCLUDE_DIR "${PROJECT_SOURCE_DIR}/../annoy/src")

if (${ENABLE_NEON})
        # ARM64 ignore mfpu option
        # https://developer.arm.com/docs/dui0774/i/compiler-command-line-options/-mfpu
	#set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3 -mfpu=neon -fprofile-use -D ENABLE_NEON")
	set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3 -fprofile-use -D ENABLE_NEON")
endif()

configure_file(
  ${CMAKE_CURRENT_SOURCE_DIR}/version.rc.in
  ${CMAKE_CURRENT_BINARY_DIR}/version.rc
  @ONLY)

if ("${CMAKE_SYSTEM_NAME}" STREQUAL "iOS")
    add_library(${PROJ_NAME} ${HEADERS}
                             ${SOURCES}
                             ${CMAKE_CURRENT_BINARY_DIR}/version.rc)
else()
    add_library(${PROJ_NAME} SHARED ${HEADERS}
                                    ${SOURCES}
                                    ${CMAKE_CURRENT_BINARY_DIR}/version.rc)
endif()

include_directories(${INCLUDE_DIR})

set(CompilerFlags
    CMAKE_CXX_FLAGS
    CMAKE_CXX_FLAGS_DEBUG
    CMAKE_CXX_FLAGS_RELEASE
    CMAKE_C_FLAGS
    CMAKE_C_FLAGS_DEBUG
    CMAKE_C_FLAGS_RELEASE
)

set(CPACK_PROJECT_NAME ${PROJECT_NAME})
set(CPACK_PROJECT_VERSION ${PROJECT_VERSION})
include(CPack)
