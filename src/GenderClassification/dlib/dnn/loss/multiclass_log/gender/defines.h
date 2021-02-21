#ifndef _CPP_LOSS_MULTICLASS_LOG_DEFINES_GENDER_DEFINES_H_
#define _CPP_LOSS_MULTICLASS_LOG_DEFINES_GENDER_DEFINES_H_

#include <dlib/dnn.h>
#include <dlib/matrix.h>

using namespace dlib;
using namespace std;

// Developer can customize these as you want to do!!!
#pragma region type definitions

// https://github.com/GilLevi/AgeGenderDeepLearning/blob/master/gender_net_definitions/deploy.prototxt
// deploy.prototxt uses LRN (Local Response Normalization).
// But it is a bit old approach and BN (Batch Normalization) is better.
template <typename SUBNET>
using conv1 = add_layer<con_<96, 7, 7, 4, 4>, SUBNET>;
template <typename SUBNET>
using conv2 = add_layer<con_<256, 5, 5, 1, 1, 2, 2>, SUBNET>;
template <typename SUBNET>
using conv3 = add_layer<con_<384, 3, 3, 1, 1, 1, 1>, SUBNET>;
template <typename SUBNET>
using norm1 = add_layer<bn_<CONV_MODE>, SUBNET>;
template <typename SUBNET>
using norm2 = add_layer<bn_<CONV_MODE>, SUBNET>;
template <typename SUBNET>
using pool1 = add_layer<max_pool_<3, 3, 2, 2>, SUBNET>;
template <typename SUBNET>
using pool2 = add_layer<max_pool_<3, 3, 2, 2>, SUBNET>;
template <typename SUBNET>
using pool5 = add_layer<max_pool_<3, 3, 2, 2>, SUBNET>;
template <typename SUBNET>
using fc6 = add_layer<fc_<512, FC_HAS_BIAS>, SUBNET>;
template <typename SUBNET>
using fc7 = add_layer<fc_<512, FC_HAS_BIAS>, SUBNET>;
template <typename SUBNET>
using fc8 = add_layer<fc_<2, FC_HAS_BIAS>, SUBNET>;
using gender_train_type = loss_multiclass_log<fc8<
							                  dropout<
							                  relu<fc7<dropout<
							                  relu<fc6<pool5<
							                  relu<conv3<norm2<pool2<
							                  relu<conv2<norm1<pool1<
							                  relu<conv1<input_rgb_image_sized<227>
                                              >>>>>>>>>>>>>>>>>>>;

static const std::vector<const char *>* gender_train_type_labels = new std::vector<const char *>(
{
    "Male", "Female"
});

#pragma endregion type definitions

#endif