#ifndef _CPP_LOSS_MULTICLASS_LOG_DEFINES_EMOTION_DEFINES_H_
#define _CPP_LOSS_MULTICLASS_LOG_DEFINES_EMOTION_DEFINES_H_

#include <dlib/dnn.h>
#include <dlib/matrix.h>

using namespace dlib;
using namespace std;

// Developer can customize these as you want to do!!!
#pragma region type definitions

// https://arxiv.org/pdf/1812.04510.pdf
template <typename SUBNET>
using fc1 = add_layer<fc_<100, FC_HAS_BIAS>, SUBNET>;
template <typename SUBNET>
using fc2 = add_layer<fc_<500, FC_HAS_BIAS>, SUBNET>;
template <typename SUBNET>
using fc3 = add_layer<fc_<7, FC_HAS_BIAS>, SUBNET>;

using emotion_train_type = loss_multiclass_log<relu<fc3<dropout<
                                               relu<fc2<dropout<
                                               relu<fc1<input<matrix<double>>>>>>>>>>>;

static const std::vector<const char *>* emotion_train_type_labels = new std::vector<const char *>(
{
    "anger", 
	"contempt", 
	"disgust", 
	"fear", 
	"happiness", 
	"neutrality", 
	"sadness", 
	"surprise"
});

#pragma endregion type definitions

#endif