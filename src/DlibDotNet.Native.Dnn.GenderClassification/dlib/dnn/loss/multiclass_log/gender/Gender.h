#ifndef _CPP_LOSS_MULTICLASS_LOG_GENDER_H_
#define _CPP_LOSS_MULTICLASS_LOG_GENDER_H_

#include <dlib/dnn.h>
#include <dlib/matrix.h>

#include "DlibDotNet.Native/dlib/export.h"
#include "DlibDotNet.Native/dlib/shared.h"
#include "defines.h"
#include "DlibDotNet.Native.Dnn/dlib/dnn/loss/multiclass_log/template.h"

typedef unsigned long gender_out_type;
typedef unsigned long gender_train_label_type;

MAKE_LOSSMULTICLASSLOG_FUNC(gender_train_type,  matrix_element_type::RgbPixel, dlib::rgb_pixel, matrix_element_type::UInt32, gender_train_label_type, 100)
MAKE_LOSSMULTICLASSLOG_FUNC(gender_test_type,   matrix_element_type::RgbPixel, dlib::rgb_pixel, matrix_element_type::UInt32, gender_train_label_type, 101)

#endif