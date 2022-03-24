import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';



@Injectable({
  providedIn: 'root'
})
export class MembersService {

  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  userParams: UserParams;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
    this.userParams = new UserParams(this.user)
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams(){
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  getMembers(userParams: UserParams) {
    var key = Object.values(userParams).join('-');
    var responce = this.memberCache.get(key);
    if (responce) {
      return of(responce);
    }
    let params = this.GetPaginationHeaders(userParams);

    return this.getPaginatedResult<Member[]>(this.baseUrl + 'users', params)
      .pipe(
        map(res => {
          this.memberCache.set(key, res);
          return res
        }));
  }


  getMember(username: string) {
    const member = [...this.memberCache.values()]
      .reduce((prevValue, currentValue) => {
        return prevValue.concat(currentValue.result);
      }, [])
      .find((member: Member) => member.username === username);
    if (member) return of(member);
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users/', member).pipe(
      take(1),
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photoId}`);
  }

  private getPaginatedResult<T>(url, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params, })
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

  private GetPaginationHeaders({ pageNumber, pageSize, gender, minAge, maxAge, orderBy }: UserParams) {
    let params = new HttpParams();
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
    params = params.append('gender', gender.toString());
    params = params.append('minAge', minAge.toString());
    params = params.append('maxAge', maxAge.toString());
    params = params.append('orderBy', orderBy.toString());
    return params;
  }
}
