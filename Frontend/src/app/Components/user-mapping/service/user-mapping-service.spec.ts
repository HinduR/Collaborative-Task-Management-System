import { provideHttpClient } from "@angular/common/http";
import {
  HttpTestingController,
  provideHttpClientTesting,
} from "@angular/common/http/testing";
import { TestBed } from "@angular/core/testing";

import { environment } from "../../../../environment/environment";
import { UserMappingService } from "./user-mapping-service";

describe("UserMappingService", () => {
  let service: UserMappingService;
  let httpTestingController: HttpTestingController;

  const identityUrl = environment.gatewayUrl + environment.endpoints.identity;
  const projectUrl = environment.gatewayUrl + environment.endpoints.project;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(UserMappingService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it("maps users to dropdown options without search params", () => {
    service.getUsers().subscribe((users) => {
      expect(users).toEqual([
        {
          label: "Ada Lovelace",
          value: "user-1",
        },
      ]);
    });

    const request = httpTestingController.expectOne(
      `${identityUrl}/users/list`,
    );

    expect(request.request.method).toBe("GET");
    expect(request.request.params.keys()).toEqual([]);
    request.flush({
      items: [
        {
          id: "user-1",
          displayName: "Ada Lovelace",
        },
      ],
    });
  });

  it("trims and sends user search params", () => {
    service.getUsers("  ada  ").subscribe();

    const request = httpTestingController.expectOne(
      (req) =>
        req.url === `${identityUrl}/users/list` &&
        req.params.get("search") === "ada",
    );

    expect(request.request.method).toBe("GET");
    request.flush({ items: [] });
  });

  it("maps projects to dropdown options", () => {
    service.getProjects().subscribe((projects) => {
      expect(projects).toEqual([
        {
          label: "Round Table",
          value: "project-1",
        },
      ]);
    });

    const request = httpTestingController.expectOne(
      `${projectUrl}/admin/projects`,
    );

    expect(request.request.method).toBe("GET");
    request.flush([
      {
        id: "project-1",
        projectName: "Round Table",
      },
    ]);
  });

  it("gets the raw project list", () => {
    const response = [
      {
        id: "project-1",
        projectName: "Round Table",
        isAssigned: true,
      },
    ];

    service.getProjectList().subscribe((projects) => {
      expect(projects).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${projectUrl}/admin/projects`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("maps roles to dropdown options", () => {
    service.getRoles().subscribe((roles) => {
      expect(roles).toEqual([
        {
          label: "Admin",
          value: "role-1",
        },
      ]);
    });

    const request = httpTestingController.expectOne(`${identityUrl}/role`);

    expect(request.request.method).toBe("GET");
    request.flush([
      {
        id: "role-1",
        name: "Admin",
      },
    ]);
  });

  it("gets user project mappings", () => {
    const response = [
      {
        userId: "user-1",
        projectIds: ["project-1"],
      },
    ];

    service.getUserProjectMappings().subscribe((mappings) => {
      expect(mappings).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${projectUrl}/admin/user-project-mappings`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("saves user project mappings", () => {
    service
      .saveUserProjectMapping("user-1", ["project-1", "project-2"])
      .subscribe((response) => {
        expect(response).toBeNull();
      });

    const request = httpTestingController.expectOne(
      `${projectUrl}/admin/users/user-1/projects`,
    );

    expect(request.request.method).toBe("PUT");
    expect(request.request.body).toEqual({
      projectIdList: ["project-1", "project-2"],
    });
    request.flush(null);
  });

  it("assigns a role to a user", () => {
    const response = {
      userId: "user-1",
      roleId: "role-1",
      roleName: "Admin",
    };

    service.assignUserRole("user-1", "role-1").subscribe((mapping) => {
      expect(mapping).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${identityUrl}/users/user-1/role`,
    );

    expect(request.request.method).toBe("PUT");
    expect(request.request.body).toEqual({
      roleId: "role-1",
    });
    request.flush(response);
  });
});
