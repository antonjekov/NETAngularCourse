import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { PaginatedResult } from "../_models/pagination";

export function getPaginatedResult<T>(url: string, params: HttpParams, http: HttpClient) {
  const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
  return http.get<T>(url, { observe: 'response', params, })
    .pipe(
      map(responce => {
        paginatedResult.result = responce.body;
        if (responce.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(responce.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
}

export function GetPaginationHeaders(pageNumber: number, pageSize: number) {
  let params = new HttpParams();
  params = params.append('pageNumber', pageNumber.toString());
  params = params.append('pageSize', pageSize.toString());
  return params;
}
