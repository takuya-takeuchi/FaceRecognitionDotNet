#ifndef _CPP_ANNOYSEARCH_SEARCH_H_
#define _CPP_ANNOYSEARCH_SEARCH_H_

#include "../../../DlibDotNet/src/DlibDotNet.Native/dlib/export.h"
#include "../../../DlibDotNet/src/DlibDotNet.Native/dlib/shared.h"

#ifndef ANNOYLIB_MULTITHREADED_BUILD
#define ANNOYLIB_MULTITHREADED_BUILD
#endif

#include "annoylib.h"
#include "kissrandom.h"

using namespace Annoy;

using AnnoyIndexEuclidean = AnnoyIndex<int, double, Euclidean, Kiss32Random, AnnoyIndexMultiThreadedBuildPolicy>;

DLLEXPORT const AnnoyIndexEuclidean* AnnoySearch_AnnoyIndex_new(int32_t f)
{
    return new AnnoyIndexEuclidean(f);
}

DLLEXPORT void AnnoySearch_AnnoyIndex_delete(AnnoyIndexEuclidean* index)
{
	delete index;
}

DLLEXPORT void AnnoySearch_AnnoyIndex_add_item(AnnoyIndexEuclidean* const index,
                                               const int32_t item,
                                               double* vector)
{
	index->add_item(item, vector);
}

DLLEXPORT void AnnoySearch_AnnoyIndex_build(AnnoyIndexEuclidean* const index,
                                            const int32_t q,
                                            const int32_t n_threads=-1)
{
	index->build(q, n_threads);
}

DLLEXPORT void AnnoySearch_AnnoyIndex_get_nns_by_vector(AnnoyIndexEuclidean* const index,
                                                        const double* query,
                                                        const size_t n,
                                                        const int32_t search_k,
                                                        std::vector<int32_t>* const toplist,
                                                        std::vector<double>* const distances)
{
	index->get_nns_by_vector(query, n, search_k, toplist, distances);
}

#endif